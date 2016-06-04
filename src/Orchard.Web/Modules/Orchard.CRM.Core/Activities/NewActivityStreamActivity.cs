using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Activities
{
    public class NewActivityStreamActivity : Event
    {
        public const string ActivityStreamRecordKey = "ActivityStream";
        public const string ActivityStreamActivityName = "NewActivityStream";
        private readonly IContentManager contentManager;

        public NewActivityStreamActivity(IContentManager contentManager)
        {
            this.contentManager = contentManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public override bool CanStartWorkflow
        {
            get { return true; }
        }

        public override string Name
        {
            get { return ActivityStreamActivityName; }
        }

        public override LocalizedString Category
        {
            get { return T("CRM Core"); }
        }

        public override LocalizedString Description
        {
            get { return T("New Activity Stream item has been created"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done"), T("Failed") };
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return workflowContext.Tokens.ContainsKey(ActivityStreamRecordKey) && base.CanExecute(workflowContext, activityContext);
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            if (!workflowContext.Tokens.ContainsKey(NewActivityStreamActivity.ActivityStreamRecordKey))
            {
                return new[]{T("Failed") } ;
            }

            ActivityStreamRecord activityStreamRecord = (ActivityStreamRecord )workflowContext.Tokens[ NewActivityStreamActivity.ActivityStreamRecordKey];
            workflowContext.SetState<int>("ActivityStreamId", activityStreamRecord.Id);
            return new[] { T("Done") };
        }
    }
}