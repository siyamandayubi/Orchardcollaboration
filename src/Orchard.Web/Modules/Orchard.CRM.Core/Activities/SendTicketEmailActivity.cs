namespace Orchard.CRM.Core.Activities
{
    using Newtonsoft.Json.Linq;
    using Orchard.ContentManagement;
    using Orchard.Core.Title.Models;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers;
    using Orchard.Data;
    using Orchard.Email.Activities;
    using Orchard.Localization;
    using Orchard.Logging;
    using Orchard.Messaging.Services;
    using Orchard.Tokens;
    using Orchard.Users.Models;
    using Orchard.Workflows.Models;
    using System.Collections.Generic;
    using System.Linq;
    
    public class SendTicketEmailActivity : CRMSendEmailActivity
    {
        private readonly ITokenizer tokenizer;
        private IRepository<EmailTemplateRecord> emailTemplateRepository;
        private readonly IContentManager contentManager;

        public SendTicketEmailActivity(
               IContentManager contentManager,
               IMessageService messageService,
               IJobsQueueService jobsQueueService,
               IRepository<EmailTemplateRecord> emailTemplateRepository,
               ITokenizer tokenizer,
               IRepository<BusinessUnitMemberPartRecord> businessUnitMembersRepository,
               IRepository<TeamMemberPartRecord> teamMembersRepository,
               IRepository<UserPartRecord> userRepository)
            : base(messageService, jobsQueueService, businessUnitMembersRepository, teamMembersRepository, userRepository)
        {
            this.contentManager = contentManager;
            this.tokenizer = tokenizer;
            this.emailTemplateRepository = emailTemplateRepository;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public override string Form
        {
            get
            {
                return EmailTemplateActivityForm.Name;
            }
        }

        public override string Name
        {
            get { return "SendTicketEmailActivity"; }
        }

        public override LocalizedString Category
        {
            get { return T("CRM Core"); }
        }

        public override LocalizedString Description
        {
            get { return T("Send email to the people who is related to the ticket"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done"), T("Failed") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            TicketPart ticketPart = workflowContext.Content.As<TicketPart>();
            CRMCommentPart commentPart = workflowContext.Content.As<CRMCommentPart>();
            ContentItemPermissionPart permissionsPart = workflowContext.Content.As<ContentItemPermissionPart>();

            if (ticketPart == null)
            {
                if (commentPart == null)
                {
                    this.Logger.Debug("ContentItem mismatch: Expexting TicketPart");
                    return new[] { T("Failed") };
                }
                else
                {
                    var contentPart = this.contentManager.Get(commentPart.Record.CRMCommentsPartRecord.ContentItemRecord.Id);
                    ticketPart = contentPart.As<TicketPart>();
                    permissionsPart = contentPart.As<ContentItemPermissionPart>();
                }
            }

            if (permissionsPart == null)
            {
                this.Logger.Debug("ContentItem mismatch: Expexting ContentItemPermissionPart ");
                return new[] { T("Failed") };
            }

            // setup tokenizer
            Dictionary<string, object> temp = new Dictionary<string, object>();
            temp.Add("Content", workflowContext.Content);
            var titlePart = ticketPart.As<TitlePart>();

            // read email template Id
            string emailTemplateIdString = activityContext.GetState<string>(EmailTemplateActivityForm.EmailTemplateIdFieldName);
            int emailTemplateId;
            if (!int.TryParse(emailTemplateIdString, out emailTemplateId))
            {
                this.Logger.Debug("There is no email Template for new Tickets");
                return new[] { T("Failed") };
            }

            bool sendToRequestingUser = this.GetBooleanValue(activityContext, EmailTemplateActivityForm.SentToRequestingUserFieldName);
            bool sendToOwner = this.GetBooleanValue(activityContext, EmailTemplateActivityForm.SentToOwnerFieldName);
            bool sendToSharedWithUsers = this.GetBooleanValue(activityContext, EmailTemplateActivityForm.SentToSharedUsersFieldName);

            var ticketEmailTemplate = this.emailTemplateRepository.Table.First(c => c.Id == emailTemplateId);

            if (ticketEmailTemplate == null)
            {
                this.Logger.Debug("There is no email Template for new Tickets");
                return new[] { T("Failed") };
            }

            var queued = activityContext.GetState<bool>("Queued");
            var priority = activityContext.GetState<int>("Priority");

            List<UserPartRecord> recipients = new List<UserPartRecord>();

            if (permissionsPart.Record.Items != null)
            {
                foreach (var permission in permissionsPart.Record.Items)
                {
                    if (permission.AccessType == ContentItemPermissionAccessTypes.Assignee && sendToOwner)
                    {
                        recipients.AddRange(this.GetRecipients(permission));
                    }
                    else if (sendToSharedWithUsers)
                    {
                        recipients.AddRange(this.GetRecipients(permission));
                    }
                }
            }

            if (sendToRequestingUser)
            {
                var record = ticketPart.Record;
                if (record.RequestingUser != null)
                {
                    recipients.Add(record.RequestingUser);
                }
                else if (record.SourceId == TicketSourceTypes.Email &&
                       !string.IsNullOrEmpty(record.SourceData))
                {
                    dynamic sender = JObject.Parse(record.SourceData);
                    string email = sender.Email;
                    string name = sender.Name;
                    recipients.Add(new UserPartRecord { Email = email, UserName = name });
                }
            }

            // filter the redundent items
            recipients = recipients.GroupBy(c => c.Id).Select(c => c.First()).ToList();

            foreach (var recipient in recipients.Where(c => !string.IsNullOrEmpty(c.Email)))
            {
                temp[ActivityTokenProvider.UserKey] = recipient;
                string body = this.tokenizer.Replace(ticketEmailTemplate.Body, temp);
                string subject = this.tokenizer.Replace(ticketEmailTemplate.Subject, temp);
                this.SendEmail(subject, body, recipient.Email, queued, priority);
            }

            return new[] { T("Done") };
        }

        private bool GetBooleanValue(ActivityContext activityContext, string name)
        {
            bool returnValue = false;

            string fieldValue = activityContext.GetState<string>(name);

            bool.TryParse(fieldValue, out returnValue);

            return returnValue;
        }
    }
}