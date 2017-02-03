using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Activities
{
    public class PermissionCreatedActivity : Event
    {
        public Localizer T { get; set; }

        public PermissionCreatedActivity()
        {
            this.T = NullLocalizer.Instance;
        }

        public override bool CanStartWorkflow
        {
            get { return true; }
        }

        public override string Name { get { return "PermissionCreated"; } }

        public override LocalizedString Category
        {
            get { return T("CRM Core"); }
        }

        public override Localization.LocalizedString Description
        {
            get { return T("The event activity will be fired in case of creation of a new Permission"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done") };
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return T("Done");
        }
    }
}