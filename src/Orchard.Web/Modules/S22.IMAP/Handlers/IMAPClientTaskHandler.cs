
namespace S22.IMAP.Handlers
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Orchard;
    using Orchard.ContentManagement;
    using Orchard.Core.Common.Models;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Logging;
    using Orchard.Security;
    using Orchard.Tasks.Scheduling;
    using Orchard.Users.Models;
    using Orchard.Workflows.Services;
    using S22.Imap.Provider;
    using S22.IMAP.Activities;
    using S22.IMAP.Models;
    using S22.IMAP.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;

    /// <summary>
    /// Take a look at http://stackoverflow.com/questions/8916146/scheduled-tasks-using-orchard-cms
    /// and http://stackoverflow.com/questions/11475187/how-to-run-scheduled-tasks-in-orchard
    /// </summary>
    public class IMAPClientTaskHandler : IScheduledTaskHandler
    {
        private const string TaskType = "IMAPClient";
        private const int PeriodInMinutes = 2;

        private readonly IIMapProviderFactory imapProviderFactory;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IOrchardServices orchardServices;
        private readonly ITransactionManager transactionManager;
        private readonly IIMAPHostRecordService imapHostRecordService;
        private readonly IWorkflowManager workflowManager;
        private IRepository<UserPartRecord> userRepository;
        public ILogger Logger { get; set; }

        public Localizer T { get; set; }

        public IMAPClientTaskHandler(
            IRepository<UserPartRecord> userRepository,
            IWorkflowManager workflowManager,
            IIMAPHostRecordService imapHostRecordService,
            IOrchardServices orchardServices,
            ITransactionManager transactionManager,
            IIMapProviderFactory imapProviderFactory,
            IScheduledTaskManager taskManager)
        {
            this.userRepository = userRepository;
            this.transactionManager = transactionManager;
            this.workflowManager = workflowManager;
            this.imapHostRecordService = imapHostRecordService;
            this.orchardServices = orchardServices;
            this.imapProviderFactory = imapProviderFactory;
            _taskManager = taskManager;
            Logger = NullLogger.Instance;
            this.T = NullLocalizer.Instance;

            try
            {
                DateTime firstDate = DateTime.UtcNow.AddMinutes(PeriodInMinutes);
                ScheduleNextTask(firstDate);
            }
            catch (Exception e)
            {
                this.Logger.Error(e, e.Message);
            }
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TaskType)
            {
                var imapSetting = this.orchardServices.WorkContext.CurrentSite.As<IMAPSettingPart>();

                this.transactionManager.Demand();
                try
                {
                    var client = this.imapProviderFactory.Create();
                    if (client != null)
                    {
                        var hostRecord = this.imapHostRecordService.Get(imapSetting.Host);

                        if (hostRecord == null)
                        {
                            hostRecord = this.imapHostRecordService.Create(imapSetting.Host, 0, DateTime.UtcNow);
                            this.imapHostRecordService.Save(hostRecord);
                        }

                        var emailIds = hostRecord.EmailUid != 0 ? client.Search(SearchCondition.GreaterThan((uint)hostRecord.EmailUid)) :
                            client.Search(SearchCondition.SentSince(hostRecord.FromDate));

                        emailIds = emailIds.Where(c => c > hostRecord.EmailUid);

                        // read first 10 emails. The reason is preventing reading thousands of emails at once
                        foreach (var uid in emailIds.Take(10))
                        {
                            var email = client.GetMessage(uid, (part) =>
                            {
                                Int64 fiveMegabytes = (1024 * 1024 * 5);
                                if (part.Size > fiveMegabytes)
                                {
                                    // Don't download this attachment.
                                    return false;
                                }

                                return true;
                            });

                            RaiseWorkflow(hostRecord, email, uid);
                        }

                        imapSetting.LastSuccessfullConnectionTime = DateTime.UtcNow;
                        imapSetting.LatestError = null;
                        imapSetting.LatestErrorTime = null;
                    }
                }
                catch (Exception e)
                {
                    this.transactionManager.Cancel();

                    this.Logger.Error(e, e.Message);

                    // We need a new transaction for storing the imapSetting
                    this.transactionManager.RequireNew();
                    imapSetting.LatestError = e.Message;
                    imapSetting.LatestErrorTime = DateTime.UtcNow;
                    var imapSettingContentItem = this.orchardServices.ContentManager.Get(imapSetting.Id);
                    var imapSettingPart1 = imapSettingContentItem.As<IMAPSettingPart>();
                    imapSettingPart1.LatestError = e.Message;
                    imapSettingPart1.LatestErrorTime = imapSetting.LatestErrorTime;
                    this.transactionManager.Demand();
                }
                finally
                {
                    DateTime nextTaskDate = DateTime.UtcNow.AddMinutes(PeriodInMinutes);
                    this.ScheduleNextTask(nextTaskDate);
                }
            }
        }

        private void RaiseWorkflow(IMAPHostRecord hostRecord, MailMessage mail, uint id)
        {
            var contentManager = this.orchardServices.ContentManager;
            var emailContentItem = contentManager.New(IMAPEmailPart.ContentItemTypeName);
            var emailPart = emailContentItem.As<IMAPEmailPart>();

            // I don't know why it is null
            if (emailPart == null)
            {
                emailPart = new IMAPEmailPart();
                emailContentItem.Weld(emailPart);
            }

            emailPart.MailMessage = mail;
            dynamic sender = new JObject();
            sender.Name = mail.From.DisplayName;
            sender.Email = mail.From.Address;
            var user = this.userRepository.Table.FirstOrDefault(c => c.Email.ToLower() == mail.From.Address.ToLower());

            if (user != null)
            {
                var commonPart = emailContentItem.As<CommonPart>();
                commonPart.Owner = contentManager.Get<IUser>(user.Id);
                sender.UserId = user.Id;
                sender.UserName = user.UserName;
            }

            emailPart.From = JsonConvert.SerializeObject(sender);
            emailPart.Subject = string.IsNullOrEmpty(mail.Subject) ? this.T("[No subject]").Text : mail.Subject;
            emailPart.UId = id;

            contentManager.Create(emailContentItem);
            contentManager.Publish(emailContentItem);

            workflowManager.TriggerEvent(
                IMapEmailReceivedActivity.ActivityName,
                emailContentItem,
                () => new Dictionary<string, object> { { "Content", emailContentItem } });

            hostRecord.EmailUid = id;
            this.imapHostRecordService.Save(hostRecord);
        }

        private void ScheduleNextTask(DateTime date)
        {
            if (date > DateTime.UtcNow)
            {
                var tasks = this._taskManager.GetTasks(TaskType);
                if (tasks == null || tasks.Count() == 0)
                    this._taskManager.CreateTask(TaskType, date, null);
            }
        }
    }
}