using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System.Collections.Generic;

namespace OC.GITConnector.Tickets.Activities
{
    public class GITCommitConnectedToTicketActivity : Event
    {
        public const string ActivityName = "GITCommitConnectedToTicket";

        public override bool CanStartWorkflow
        {
            get { return true; }
        }
        
        public Localizer T { get; set; }
        
        public override string Name
        {
            get { return ActivityName; }
        }

        public override LocalizedString Description
        {
            get { return T("An GIT log has been connected to a ticket"); }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return T("Done");
        }

        public override LocalizedString Category
        {
            get { return T("GIT"); }
        }
    }
}