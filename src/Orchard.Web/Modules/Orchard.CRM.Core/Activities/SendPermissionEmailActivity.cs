using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers;
using Orchard.CRM.Core.Services;
using Orchard.Data;
using Orchard.DisplayManagement.Shapes;
using Orchard.Email.Activities;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Messaging.Services;
using Orchard.Tokens;
using Orchard.Users.Models;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Activities
{
    public class SendPermissionEmailActivity : CRMSendEmailActivity
    {
        private readonly ITokenizer tokenizer;
        private IRepository<EmailTemplateRecord> emailTemplateRepository;

        public SendPermissionEmailActivity(
               IMessageService messageService,
               IJobsQueueService jobsQueueService,
               IRepository<EmailTemplateRecord> emailTemplateRepository,
               ITokenizer tokenizer,
               IRepository<BusinessUnitMemberPartRecord> businessUnitMembersRepository,
               IRepository<TeamMemberPartRecord> teamMembersRepository,
               IRepository<UserPartRecord> userRepository)
            : base(messageService, jobsQueueService, businessUnitMembersRepository, teamMembersRepository, userRepository)
        {
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
            get { return "SendPermissionEmailActivity"; }
        }

        public override LocalizedString Category
        {
            get { return T("CRM Core"); }
        }

        public override LocalizedString Description
        {
            get { return T("Send email to the people who is related to the Permission in the list of the tokens of the workflow"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done"), T("Failed") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            TicketPart ticketPart = workflowContext.Content.As<TicketPart>();
            if (!workflowContext.Tokens.ContainsKey("Permission"))
            {
                this.Logger.Debug("There is no Permission among the tokens");
                return new[] { T("Failed") };
            }

            ContentItemPermissionDetailRecord permission = (ContentItemPermissionDetailRecord)workflowContext.Tokens["Permission"];

            if (permission == null)
            {
                this.Logger.Debug("There is no Permission among the tokens");
                return new[] { T("Failed") };
            }

            if (ticketPart == null)
            {
                this.Logger.Debug("ContentItem mismatch: Expexting TicketPart");
                return new[] { T("Failed") };
            }

            // setup tokenizer
            Dictionary<string, object> temp = new Dictionary<string, object>();
            temp.Add(ActivityTokenProvider.PermissionDetailKey, permission);
            var titlePart = ticketPart.As<TitlePart>();
            if (titlePart != null)
            {
                temp.Add(ActivityTokenProvider.TitkeKey, titlePart);
            }

            CRMCommentPart commentPart = workflowContext.Content.As<CRMCommentPart>();
            if (commentPart != null)
            {
                temp.Add(ActivityTokenProvider.CRMCommentKey, commentPart);
            }

            string emailTemplateIdString = activityContext.GetState<string>(EmailTemplateActivityForm.EmailTemplateIdFieldName);
            int emailTemplateId;
            if (!int.TryParse(emailTemplateIdString, out emailTemplateId))
            {
                this.Logger.Debug("There is no email Template for new Tickets");
                return new[] { T("Failed") };
            }

            var ticketEmailTemplate = this.emailTemplateRepository.Table.First(c => c.Id == emailTemplateId);

            if (ticketEmailTemplate == null)
            {
                this.Logger.Debug("There is no email Template for new Tickets");
                return new[] { T("Failed") };
            }

            var queued = activityContext.GetState<bool>("Queued");
            var priority = activityContext.GetState<int>("Priority");

            var recipients = this.GetRecipients(permission);
            foreach (var recipient in recipients.Where(c => !string.IsNullOrEmpty(c.Email)))
            {
                temp.Add(ActivityTokenProvider.UserKey, recipient);
                string body = this.tokenizer.Replace(ticketEmailTemplate.Body, temp);
                string subject = this.tokenizer.Replace(ticketEmailTemplate.Subject, temp);
                this.SendEmail(subject, body, recipient.Email, queued, priority);
            }

            return new[] { T("Done") };
        }
    }
}