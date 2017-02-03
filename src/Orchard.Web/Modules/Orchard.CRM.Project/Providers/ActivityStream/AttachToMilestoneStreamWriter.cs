namespace Orchard.CRM.Project.Providers.ActivityStream
{
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Records;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.Settings;
    using Orchard.CRM.Project.Models;
    using Orchard.CRM.Project.Services;
    using Orchard.Localization;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    
    public class AttachToMilestoneStreamWriter : IActivityStreamWriter
    {
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;
        private readonly IMilestoneService milestoneService;

        public AttachToMilestoneStreamWriter(IContentItemDescriptorManager contentItemDescriptorManager, IMilestoneService milestoneService)
        {
            this.milestoneService = milestoneService;
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            var attachToMilestone = context.ContentItem.As<AttachToMilestonePart>();
            bool result = attachToMilestone != null && (context.Snapshot != null);

            return result;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                yield break;
            }

            var attachToMilestonePart = context.ContentItem.As<AttachToMilestonePart>();

            int? newMilestoneId = attachToMilestonePart.Record.MilestoneId;
            int? oldMilestoneId = context.Snapshot != null ? context.Snapshot.MilestoneId: null;

            var newMilestone = newMilestoneId.HasValue?
                this.milestoneService.GetMilestone(newMilestoneId.Value).As<MilestonePart>()
                : null;

            if (oldMilestoneId == null && newMilestoneId == null)
            {
                yield break;
            }
            else if (oldMilestoneId == null && newMilestoneId != null)
            {
                string change = T(
                    "{0} is added to the '{1}'", 
                    contentItemDescriptorManager.GetDescription(context.ContentItem), 
                    contentItemDescriptorManager.GetDescription(newMilestone)).Text;

                yield return new ActivityStreamChangeItem(change);
            }
            else if (oldMilestoneId != null && newMilestoneId == null)
            {
                var oldMilestone = this.milestoneService.GetMilestone(oldMilestoneId.Value).As<MilestonePart>();
                string change = T(
                    "{0} is detached from the '{1}'", 
                    contentItemDescriptorManager.GetDescription(context.ContentItem),
                    contentItemDescriptorManager.GetDescription(oldMilestone)).Text;
                yield return new ActivityStreamChangeItem(change);
            }
            else if (oldMilestoneId != null && newMilestoneId != null && oldMilestoneId.Value != newMilestoneId.Value)
            {
                var oldContentItem = this.milestoneService.GetMilestone(oldMilestoneId.Value);

                string change = T(
                    "{0} is detached from the '{1}'", 
                    contentItemDescriptorManager.GetDescription(context.ContentItem), 
                    contentItemDescriptorManager.GetDescription(oldContentItem)).Text;
                yield return new ActivityStreamChangeItem(change);

                change = T(
                    "{0} is attached to the '{1}'", 
                    contentItemDescriptorManager.GetDescription(context.ContentItem), 
                    contentItemDescriptorManager.GetDescription(newMilestone)).Text;
                yield return new ActivityStreamChangeItem(change);
            }

            if (newMilestoneId.HasValue)
            {
                int oldOrder = context.Snapshot.OrderId;
                int? size = context.Snapshot.Size;

                if (oldOrder != attachToMilestonePart.Record.OrderId)
                {
                    string change = T(
                        "{0} is moved to the position '{1}' in '{2}'",
                        contentItemDescriptorManager.GetDescription(context.ContentItem),
                        attachToMilestonePart.Record.OrderId.ToString(CultureInfo.InvariantCulture),
                        contentItemDescriptorManager.GetDescription(newMilestone)).Text;
                    yield return new ActivityStreamChangeItem(change, true, newMilestone.ContentItem.Id, newMilestone.ContentItem.VersionRecord.Id);
                }

                if (size != attachToMilestonePart.Record.Size)
                {
                    string change = T(
                        "The Size of '{0}' in the milestone '{1}' is changed to'{2}'",
                        contentItemDescriptorManager.GetDescription(context.ContentItem),
                        contentItemDescriptorManager.GetDescription(newMilestone),
                        attachToMilestonePart.Record.Size.ToString()).Text;
                    yield return new ActivityStreamChangeItem(change, true, newMilestone.ContentItem.Id, newMilestone.ContentItem.VersionRecord.Id);
                }
            }
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            return null;
        }

        public string Name
        {
            get { return "AttachToMilestone"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            var part = contentItem.As<AttachToMilestonePart>();

            if (part == null)
            {
                return null;
            }

            dynamic oldData = new ExpandoObject();
            var oldDataDictionary = oldData as IDictionary<string, object>;

            oldData.VersionId = contentItem.VersionRecord.Id;
            oldData.ContentItemId = part.ContentItem.Id;
            oldData.MilestoneId = part.Record.MilestoneId;
            oldData.OrderId = part.Record.OrderId;
            oldData.Size = part.Record.Size;
            return oldData;
        }
    }
}