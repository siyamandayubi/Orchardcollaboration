using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.Settings;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.ActivityStream
{
    public class BodyPartStreamWriter : IActivityStreamWriter
    {
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;

        public BodyPartStreamWriter(IContentItemDescriptorManager contentItemDescriptorManager)
        {
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            return context.ContentItem.As<BodyPart>() != null;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                yield break;
            }

            var newPart = context.ContentItem.As<BodyPart>();
            var oldData = context.Snapshot;

            if (oldData == null)
            {
                yield break;
            }

            // if new part is null
            if (string.IsNullOrEmpty(newPart.Text) && !string.IsNullOrEmpty(oldData.Text))
            {
                yield return new ActivityStreamChangeItem(T("Set the content text to null").Text);
            }
            else if (newPart.Text == oldData.Text)
            {
                // do nothing
                yield break;
            }
            else if (!string.IsNullOrEmpty(newPart.Text))
            {
                yield return new ActivityStreamChangeItem(T("Updates the content text").Text);
            }
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            return null;
        }

        public string Name
        {
            get { return "Body"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            var part = contentItem.As<BodyPart>();

            if (part == null)
            {
                return null;
            }

            dynamic oldData = new ExpandoObject();
            var oldDataDictionary = oldData as IDictionary<string, object>;

            oldData.Text = part.Text;
            oldData.VersionId = contentItem.VersionRecord.Id;
            oldData.ContentItemId = part.ContentItem.Id;
            return oldData;
        }
    }
}