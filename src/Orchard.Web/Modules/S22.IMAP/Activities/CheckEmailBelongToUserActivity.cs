using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using S22.IMAP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using System.Web;
using Newtonsoft.Json.Linq;
using Orchard.Data;
using Orchard.Users.Models;

namespace S22.IMAP.Activities
{
    public class CheckEmailBelongToUserActivity : Task
    {
        public const string YesOutcome = "Yes";
        public const string NoOutcome = "No";

        private IRepository<UserPartRecord> userRepository;
        public CheckEmailBelongToUserActivity(IRepository<UserPartRecord> userRepository)
        {
            this.userRepository = userRepository;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name
        {
            get { return "CheckEmailBelongToUser"; }
        }

        public override LocalizedString Category
        {
            get { return T("Email"); }
        }

        public override LocalizedString Description
        {
            get { return T("This activity can be used along side of 'Email Received' activity. It checks whether the received email belong to a user or not."); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T(YesOutcome), T(NoOutcome) };
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var content = workflowContext.Content.ContentItem;
            if (content.ContentType != IMAPEmailPart.ContentItemTypeName)
            {
                return false;
            }

            if (content.As<IMAPEmailPart>() == null)
            {
                return false;
            }

            return true;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var content = workflowContext.Content.ContentItem;
            var emailPart = content.As<IMAPEmailPart>();
            dynamic sender = JObject.Parse(emailPart.From);
            string email = sender.Email;
            var user = this.userRepository.Table.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());

            if (user != null)
            {
                return new[] { T(YesOutcome) };
            }
            else
            {
                return new[] { T(NoOutcome) };
            }
        }
    }
}