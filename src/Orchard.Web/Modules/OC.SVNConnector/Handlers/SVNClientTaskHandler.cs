
namespace OC.SVNConnector.Handlers
{
    using Activities;
    using Models;
    using Orchard;
    using Orchard.ContentManagement;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Logging;
    using Orchard.Tasks.Scheduling;
    using Orchard.Users.Models;
    using Orchard.Workflows.Services;
    using SharpSvn;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Take a look at http://stackoverflow.com/questions/8916146/scheduled-tasks-using-orchard-cms
    /// and http://stackoverflow.com/questions/11475187/how-to-run-scheduled-tasks-in-orchard
    /// </summary>
    public class SVNClientTaskHandler : IScheduledTaskHandler
    {
        private const string TaskType = "SVNClient";
        private const int PeriodInMinutes = 2;

        private readonly IScheduledTaskManager _taskManager;
        private readonly IOrchardServices orchardServices;
        private readonly ITransactionManager transactionManager;
        private readonly IWorkflowManager workflowManager;
        private readonly IRepository<SVNServerRecord> svnServerRepository;
        private IRepository<UserPartRecord> userRepository;
        public ILogger Logger { get; set; }

        public Localizer T { get; set; }

        public SVNClientTaskHandler(
            IRepository<UserPartRecord> userRepository,
            IRepository<SVNServerRecord> svnServerRepository,
            IWorkflowManager workflowManager,
            IOrchardServices orchardServices,
            ITransactionManager transactionManager,
            IScheduledTaskManager taskManager)
        {
            this.svnServerRepository = svnServerRepository;
            this.userRepository = userRepository;
            this.transactionManager = transactionManager;
            this.workflowManager = workflowManager;
            this.orchardServices = orchardServices;
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
                var settings = this.orchardServices.WorkContext.CurrentSite.As<SVNSettingsPart>();

                this.transactionManager.Demand();
                try
                {
                    var client = new SvnClient();
                    client.Authentication.DefaultCredentials = new NetworkCredential(settings.Username, settings.Password);
                    var serverRecord = this.svnServerRepository.Table.FirstOrDefault(c => c.Server == settings.Server);

                    if (serverRecord == null)
                    {
                        serverRecord = new SVNServerRecord { Server = settings.Server, LastRevision = settings.LastRevision, FromDate = DateTime.UtcNow };
                        this.svnServerRepository.Create(serverRecord);
                        this.svnServerRepository.Flush();
                    }

                    Uri svnrepo = new Uri(settings.Server);
                    SvnInfoEventArgs info;
                    client.GetInfo(svnrepo, out info);
                    long lastRev = info.LastChangeRevision;
                    long rangeStart = serverRecord.LastRevision;
                    Collection<SvnLogEventArgs> logs = new Collection<SvnLogEventArgs>();
                    client.GetLog(svnrepo, new SvnLogArgs(new SvnRevisionRange(rangeStart, lastRev)), out logs);
                    
                    foreach (var svnLog in logs)
                    {                       
                        RaiseWorkflow(serverRecord, svnLog);
                    }

                    settings.LastSuccessfullConnectionTime = DateTime.UtcNow;
                    settings.LatestError = null;
                    settings.LatestErrorTime = null;
                }
                catch (Exception e)
                {
                    this.transactionManager.Cancel();

                    this.Logger.Error(e, e.Message);

                    // We need a new transaction for storing the data
                    this.transactionManager.RequireNew();
                    settings.LatestError = e.Message;
                    settings.LatestErrorTime = DateTime.UtcNow;
                    var settingContentItem = this.orchardServices.ContentManager.Get(settings.Id);
                    var svnSettingPart = settingContentItem.As<SVNSettingsPart>();
                    svnSettingPart.LatestError = e.Message;
                    svnSettingPart.LatestErrorTime = settings.LatestErrorTime;
                    this.transactionManager.Demand();
                }
                finally
                {
                    DateTime nextTaskDate = DateTime.UtcNow.AddMinutes(PeriodInMinutes);
                    this.ScheduleNextTask(nextTaskDate);
                }
            }
        }

        private void RaiseWorkflow(SVNServerRecord hostRecord, SvnLogEventArgs log)
        {
            var contentManager = this.orchardServices.ContentManager;
            var svnLogContentItem = contentManager.New(SVNLogPart.ContentItemTypeName);
            var svnLogPart = svnLogContentItem.As<SVNLogPart>();
            

            svnLogPart.LogMessage = log.LogMessage;
            svnLogPart.Author = log.Author;
            svnLogPart.Revision = log.Revision;
            svnLogPart.Time = log.Time;
            svnLogPart.LogOrigin = log.LogOrigin.AbsolutePath;

            contentManager.Create(svnLogContentItem);
            contentManager.Publish(svnLogContentItem);

            workflowManager.TriggerEvent(
                SVNLogReceivedActivity.ActivityName,
                svnLogContentItem,
                () => new Dictionary<string, object> { { "Content", svnLogContentItem } });

            hostRecord.LastRevision = log.Revision;
            this.svnServerRepository.Flush();
        }

        private void ScheduleNextTask(DateTime date)
        {
            if (date > DateTime.UtcNow)
            {
                this.transactionManager.RequireNew();
                var tasks = this._taskManager.GetTasks(TaskType);
                if (tasks == null || tasks.Count(c=>c.ScheduledUtc.HasValue && c.ScheduledUtc.Value > DateTime.UtcNow) == 0)
                    this._taskManager.CreateTask(TaskType, date, null);
            }
        }
    }
}