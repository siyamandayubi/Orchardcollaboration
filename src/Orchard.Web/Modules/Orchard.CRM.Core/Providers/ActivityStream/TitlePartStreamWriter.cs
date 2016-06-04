/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

using Orchard.ContentManagement;
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
    public class TitlePartStreamWriter : IActivityStreamWriter
    {
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;

        public TitlePartStreamWriter(IContentItemDescriptorManager contentItemDescriptorManager)
        {
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            return context.ContentItem.As<TitlePart>() != null;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            List<ActivityStreamChangeItem> changes = new List<ActivityStreamChangeItem>();

            var newTitlePart = context.ContentItem.As<TitlePart>();
            var oldData = context.Snapshot;

            if (oldData == null)
            {
                return null;
            }

            // if new title is null
            if (string.IsNullOrEmpty(newTitlePart.Title) && !string.IsNullOrEmpty(oldData.Title))
            {
                return new[] { new ActivityStreamChangeItem(T("Set the Title to null").Text) };
            }
            else if (newTitlePart.Title == oldData.Title)
            {
                // do nothing
                return null;
            }
            else if (!string.IsNullOrEmpty(newTitlePart.Title))
            {
                return new[] { new ActivityStreamChangeItem(T("Set the Title to '{0}'", newTitlePart.Title).Text) };
            }

            return null;
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            TitlePart part = context.ContentItem.As<TitlePart>();

            string contentType = context.ContentItem.TypeDefinition.DisplayName;

            string description = string.Empty;

            // new item
            if (context.Snapshot == null)
            {
                description = string.Format("Creates new {0} - {1}", contentType, part.Title);
            }
            else
            {
                description = string.Format("updates {0} - '{1}'",
                    contentType,
                    part.Title);
            }

            return new ActivityStreamContentDescription(this.Name)
            {
                Description = description,
                Weight = 10
            };
        }

        public string Name
        {
            get { return "Title"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            var part = contentItem.As<TitlePart>();

            if (part == null)
            {
                return null;
            }

            dynamic oldData = new ExpandoObject();
            var oldDataDictionary = oldData as IDictionary<string, object>;

            oldData.Title = part.Title;
            oldData.VersionId = contentItem.VersionRecord.Id;
            oldData.ContentItemId = part.ContentItem.Id;
            return oldData;
        }
    }
}