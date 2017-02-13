namespace Orchard.CRM.Core.Activities
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using Orchard.CRM.Core.Services;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Logging;
    using Orchard.Tokens;
    using Orchard.Workflows.Models;
    using Orchard.Workflows.Services;
    using System;
    using System.Collections.Generic;

    public class UpdateTicketActivity : Task
    {
        private readonly IContentManager contentManager;
        private readonly IRepository<ContentItemPermissionDetailRecord> contentItemPermissionDetailRepository;
        private readonly IBasicDataService basicDataService;
        protected readonly ICRMContentOwnershipService contentOwnershipService;
        protected readonly IRepository<TicketIdentityRecord> ticketIdentityRecordRepository;
        private readonly ITokenizer tokenizer;
        private readonly IActivityStreamService activityStreamService;

        public UpdateTicketActivity(
            ITokenizer tokenizer,
            ICRMContentOwnershipService contentOwnershipService,
            IContentManager contentManager,
            IBasicDataService basicDataService,
            IRepository<TicketIdentityRecord> ticketIdentityRecordRepository,
            IActivityStreamService activityStreamService,
            IRepository<ContentItemPermissionDetailRecord> contentItemPermissionDetailRepository)
        {
            this.activityStreamService = activityStreamService;
            this.tokenizer = tokenizer;
            this.ticketIdentityRecordRepository = ticketIdentityRecordRepository;
            this.contentOwnershipService = contentOwnershipService;
            this.basicDataService = basicDataService;
            this.contentItemPermissionDetailRepository = contentItemPermissionDetailRepository;
            this.contentManager = contentManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public override string Name
        {
            get { return "UpdateTicket"; }
        }

        public override string Form
        {
            get
            {
                return UpdateTicketActivityForm.Name;
            }
        }
        public override LocalizedString Category
        {
            get { return T("CRM Core"); }
        }

        public override LocalizedString Description
        {
            get { return T("Update the Ticket"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            ContentItem contentItem = workflowContext.Content.ContentItem;

            var ticketPart = contentItem.As<TicketPart>();
            var commentPart = contentItem.As<CRMCommentPart>();

            if (ticketPart == null)
            {
                if (commentPart != null)
                {
                    var ticketContentItem = this.contentManager.Get(commentPart.Record.CRMCommentsPartRecord.ContentItemRecord.Id);
                    ticketPart = ticketContentItem.As<TicketPart>();
                }

                if (ticketPart == null)
                {
                    return new[] { T("Done") };
                }
            }

            // take snapshop in order to be able to trace the changes
            var snapshot = this.activityStreamService.TakeSnapshot(ticketPart.ContentItem);

            Func<string, int?> getValue = key =>
            {
                string valueString = activityContext.GetState<string>(key);
                int value;
                return int.TryParse(valueString, out value) ? (int?)value : null;
            };

            Func<string, bool> getBooleanValue = key =>
            {
                string valueString = activityContext.GetState<string>(key);
                bool value = false;
                return bool.TryParse(valueString, out value) ? value : false;
            };

            // service
            if (getBooleanValue(UpdateTicketActivityForm.UpdateServiceId))
            {
                int? serviceId = getValue(UpdateTicketActivityForm.ServiceId);
                if (serviceId.HasValue)
                {
                    ticketPart.Record.Service = new ServicePartRecord { Id = serviceId.Value };
                }
                else
                {
                    ticketPart.Record.Service = null;
                }
            }

            // Due Date
            if (getBooleanValue(UpdateTicketActivityForm.UpdateDueDateId))
            {
                int? dueDateDays = getValue(UpdateTicketActivityForm.DueDateId);
                if (dueDateDays.HasValue)
                {
                    var dueDate = DateTime.UtcNow.AddDays(dueDateDays.Value);
                    ticketPart.Record.DueDate = dueDate;
                }
                else
                {
                    ticketPart.Record.DueDate = null;
                }
            }

            // status
            if (getBooleanValue(UpdateTicketActivityForm.UpdateStatusId))
            {
                int? statusId = getValue(UpdateTicketActivityForm.StatusId);
                if (statusId.HasValue)
                {
                    ticketPart.Record.StatusRecord = new StatusRecord { Id = statusId.Value };
                }
                else
                {
                    ticketPart.Record.StatusRecord = null;
                }
            }

            // priority
            if (getBooleanValue(UpdateTicketActivityForm.UpdatePriorityId))
            {
                int? priorityId = getValue(UpdateTicketActivityForm.PriorityId);
                if (priorityId.HasValue)
                {
                    ticketPart.Record.PriorityRecord = new PriorityRecord { Id = priorityId.Value };
                }
                else
                {
                    ticketPart.Record.PriorityRecord = null;
                }
            }

            // Title
            if (getBooleanValue(UpdateTicketActivityForm.UpdateTicketTitle))
            {
                ticketPart.Record.Title = activityContext.GetState<string>(UpdateTicketActivityForm.TicketTitle);
            }

            // Description
            ticketPart.Record.Description += activityContext.GetState<string>(UpdateTicketActivityForm.TicketDescription);

            this.contentManager.Publish(contentItem);
            this.activityStreamService.WriteChangesToStreamActivity(ticketPart.ContentItem, snapshot, true, StreamWriters.TicketStreamWriter);

            return new[] { T("Done") };
        }
    }
}