using Orchard.CRM.Core.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.CRM.Core.Activities
{
    public abstract class BasicDataBranchActivity<TBasicData> : Task
        where TBasicData : IBasicDataRecord
    {
        public abstract string ActivityName { get; }
        public abstract string UnknownValue { get; }
        public abstract string BasicDataRecordName { get; }

        private readonly IContentManager contentManager;

        public BasicDataBranchActivity(IContentManager contentManager)
        {
            this.contentManager = contentManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public override string Name { get { return this.ActivityName; } }

        public override LocalizedString Category { get { return T("CRM Core"); } }

        public override LocalizedString Description
        {
            get { return this.T("Branch the workflow based on the " + BasicDataRecordName + " of the Ticket"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(Workflows.Models.WorkflowContext workflowContext, Workflows.Models.ActivityContext activityContext)
        {
            var records = this.GetData().ToList(); ;

            var returnTypes = records.Select(c => this.T(c.Name)).ToList();

            returnTypes.Add(this.T("Failed"));
            returnTypes.Add(this.T(this.UnknownValue));

            return returnTypes;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var records = this.GetData().ToList();
            TicketPart ticketPart = workflowContext.Content.As<TicketPart>();
            CRMCommentPart commentPart = workflowContext.Content.As<CRMCommentPart>();

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
                }
            }

            var record = this.GetFromTicket(ticketPart);
            if (record == null)
            {
                this.Logger.Debug("TicketPart doesn't have any " + BasicDataRecordName);
                return new[] { T(this.UnknownValue) };
            }

            return new[] { this.T(record.Name) };
        }

        protected abstract IEnumerable<TBasicData> GetData();

        protected abstract TBasicData GetFromTicket(TicketPart ticketPart);
    }
}