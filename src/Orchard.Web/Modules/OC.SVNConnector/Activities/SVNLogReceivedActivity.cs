using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System.Collections.Generic;

namespace OC.SVNConnector.Activities
{
    public class SVNLogReceivedActivity : Event
    {
        public const string ActivityName = "SVNLogReceived";

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
            get { return T("An SVN log is received"); }
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
            get { return T("SVN"); }
        }
    }
}