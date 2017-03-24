using Orchard.CRM.Core.Providers.ActivityStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using Orchard.Localization;
using Orchard.CRM.TimeTracking.Models;
using System.Globalization;
using System.Web.Routing;

namespace Orchard.CRM.TimeTracking.Providers
{
    public class TimeTrackerStreamWriter : IActivityStreamWriter
    {
        private readonly IOrchardServices services;
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;

        public TimeTrackerStreamWriter(IOrchardServices services, IContentItemDescriptorManager contentItemDescriptorManager)
        {
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.services = services;
            this.T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            throw new NotImplementedException();
        }

        public string Name => "TimeTracking";

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            return context.ContentItem.As<TimeTrackingItemPart>() != null;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            List<string> changes = new List<string>();

            TimeTrackingItemRecord old = context.Snapshot != null ? (context.Snapshot as TimeTrackingItemRecord) : null;
            TimeTrackingItemRecord newValue = (context.ContentItem.As<TimeTrackingItemPart>()).Record;

            if (old == null)
            {
                string change = T("New Work is logged").Text;
                return new[] { new ActivityStreamChangeItem(change) };
            }

            // change OriginalTimeTrackingString
            if (old.OriginalTimeTrackingString != newValue.OriginalTimeTrackingString)
            {
                string newTitleString = !string.IsNullOrEmpty(newValue.OriginalTimeTrackingString) ? newValue.OriginalTimeTrackingString : this.T("Empty").Text;
                newTitleString = string.Format(
                    CultureInfo.CurrentUICulture,
                    T("changed the 'Worked done' to: '{0}'").Text,
                    newTitleString);

                changes.Add(newTitleString);
            }

            // change date
            if (old.TrackingDate.HasValue && newValue.TrackingDate.HasValue && old.TrackingDate.Value.Date != newValue.TrackingDate.Value.Date)
            {
                string newDescriptionString = newValue.TrackingDate.HasValue ? newValue.TrackingDate.Value.Date.ToLongDateString() : this.T("Empty").Text;
                newDescriptionString = T("Starting date of the logged work is changed").Text;

                changes.Add(newDescriptionString);
            }


            // change comment
            if (old.Comment != newValue.Comment)
            {
                string newDescriptionString = !string.IsNullOrEmpty(newValue.Comment) ? newValue.Comment : this.T("Empty").Text;
                newDescriptionString = T("changed the Description").Text;

                changes.Add(newDescriptionString);
            }

            return changes.Select(c => new ActivityStreamChangeItem(c));
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            var timeTrackingItemPart = context.ContentItem.As<TimeTrackingItemPart>();

            string projectDescription = contentItemDescriptorManager.GetDescription(context.ContentItem);

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("action", "Display");
            routeValueDictionary.Add("controller", "Item");
            routeValueDictionary.Add("area", "Orchard.CRM.Core");
            routeValueDictionary.Add("id", context.ContentItem.Id);

            ActivityStreamContentDescription returnValue = new ActivityStreamContentDescription("TiemTrackerStreamWriter") { Weight = 1, RouteValues = routeValueDictionary };

            if(context.Snapshot == null)
            {
                returnValue.Description = T("New work is logged").Text;
            }
            else
            {
                returnValue.Description = T("Logged work has been updated").Text;
            }

            return returnValue;
        }
    }
}