using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Activities
{
    /// <summary>
    /// TODO: The original Timer at Orchard.Workflows project has a bug (The Sweep method of the TimerBackgroundTask stops running the awaiting actions
    /// in case one of them raises an exception, but it must run the next awaiting workflow in case of having an exception), after fixing it in the original place, get rid of this class
    /// </summary>
    public class OrchardCollaborationTimerActivity: Event {
        private readonly IClock _clock;
        private readonly IDateLocalizationServices _dateServices;

        public const string ActionName = "OrchardCollaborationTimer";
        public OrchardCollaborationTimerActivity(IClock clock, IDateLocalizationServices dateServices)
        {
            _clock = clock;
            _dateServices = dateServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return ActionName; }
        }

        public override LocalizedString Category {
            get { return T("Tasks"); }
        }

        public override LocalizedString Description {
            get { return T("Wait until a specific time has passed."); }
        }

        public override string Form {
            get { return "ActivityTimer"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return IsExpired(workflowContext, activityContext);
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            if(IsExpired(workflowContext, activityContext)) {
                yield return T("Done");
            }
        }

        private bool IsExpired(WorkflowContext workflowContext, ActivityContext activityContext) {
            DateTime started;

            if (!workflowContext.HasStateFor(activityContext.Record, "StartedUtc")) {
                var dateString = activityContext.GetState<string>("Date");
                var date = _dateServices.ConvertFromLocalizedString(dateString);
                started = date ?? _clock.UtcNow;

                workflowContext.SetStateFor(activityContext.Record, "StartedUtc", started);
            }
            else {
                started = workflowContext.GetStateFor<DateTime>(activityContext.Record, "StartedUtc");
            }

            var amount = activityContext.GetState<int>("Amount");
            var type = activityContext.GetState<string>("Unity");

            return _clock.UtcNow > When(started, amount, type);
        }

        public static DateTime When(DateTime started, int amount, string type) {
            try {
                var when = started;

                switch (type) {
                    case "Minute":
                        when = when.AddMinutes(amount);
                        break;
                    case "Hour":
                        when = when.AddHours(amount);
                        break;
                    case "Day":
                        when = when.AddDays(amount);
                        break;
                    case "Week":
                        when = when.AddDays(7*amount);
                        break;
                    case "Month":
                        when = when.AddMonths(amount);
                        break;
                    case "Year":
                        when = when.AddYears(amount);
                        break;
                }

                return when;
            }
            catch {
                return DateTime.MaxValue;
            }
        }
    }


    /// <summary>
    /// TODO: The original Timer at Orchard.Workflows project has a bug (The Sweep method of the TimerBackgroundTask stops running the awaiting actions
    /// in case one of them raises an exception, but it must run the next awaiting workflow in case of having an exception), after fixing it in the original place, get rid of this class
    /// </summary>
    public class OrchardCollaborationTimerBackgroundTask : IBackgroundTask
    {
        private readonly IContentManager _contentManager;
        private readonly IWorkflowManager _workflowManager;
        private readonly IRepository<AwaitingActivityRecord> _awaitingActivityRepository;
        private readonly string _shellName;

        public OrchardCollaborationTimerBackgroundTask(
            IContentManager contentManager,
            IWorkflowManager workflowManager,
            ShellSettings shellSettings,
           IRepository<AwaitingActivityRecord> awaitingActivityRepository)
        {
            _shellName = shellSettings.Name;
            _contentManager = contentManager;
            _workflowManager = workflowManager;
            _awaitingActivityRepository = awaitingActivityRepository;
            Logger = NullLogger.Instance;
        }
 
        public ILogger Logger { get; set; }

        public void Sweep()
        {
            var awaiting = _awaitingActivityRepository.Table.Where(x => x.ActivityRecord.Name == OrchardCollaborationTimerActivity.ActionName).ToList();


            foreach (var action in awaiting)
            {
                try
                {
                    var contentItem = _contentManager.Get(action.WorkflowRecord.ContentItemRecord.Id, VersionOptions.Latest);
                    var tokens = new Dictionary<string, object> { { "Content", contentItem } };
                    var workflowState = FormParametersHelper.FromJsonString(action.WorkflowRecord.State);
                    workflowState.TimerActivity_StartedUtc = null;
                    action.WorkflowRecord.State = FormParametersHelper.ToJsonString(workflowState);
                    _workflowManager.TriggerEvent(OrchardCollaborationTimerActivity.ActionName, contentItem, () => tokens);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "OrchardCollaborationTimerBackgroundTask: Error while processing background task \"{0}\" on tenant \"{1}\".", action.ActivityRecord.Name, _shellName);
                }
            }
        }
    }
}