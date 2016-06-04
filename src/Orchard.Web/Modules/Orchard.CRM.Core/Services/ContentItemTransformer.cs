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

using NHibernate.Transform;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Orchard.CRM.Core.Services
{
    public class ContentItemTransformer : IResultTransformer
    {
        private ContentTypeDefinition contentTypeDefinition;
        private IPartTypeRecordMatchingService partTypeRecordMatchingService;

        private Func<string, ContentItem> createContentItem;

        public ContentItemTransformer(ContentTypeDefinition contentTypeDefinition, IPartTypeRecordMatchingService partTypeGenricsService, Func<string, ContentItem> createContentItem)
        {
            this.createContentItem = createContentItem;
            this.partTypeRecordMatchingService = partTypeGenricsService;
            this.contentTypeDefinition = contentTypeDefinition;
        }

        public IList TransformList(IList collection)
        {
            return collection;
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            return Transform(tuple);
        }

        private ContentItem Transform(object[] tuple)
        {
            ContentItemVersionRecord contentItemVersionRecord = null;
            ContentItemRecord contentItemRecord = null;
            List<ContentPartRecord> contentParts = new List<ContentPartRecord>();

            foreach (var item in tuple)
            {
                if (item is ContentItemRecord)
                {
                    contentItemRecord = item as ContentItemRecord;
                }
                else if (item is ContentItemVersionRecord)
                {
                    contentItemVersionRecord = item as ContentItemVersionRecord;
                }
                else if (item is ContentPartRecord)
                {
                    contentParts.Add(item as ContentPartRecord);
                }
            }

            if (contentItemRecord != null && contentItemVersionRecord != null)
            {
                contentItemVersionRecord.ContentItemRecord = contentItemRecord;
            }

            ContentItem contentItem = this.createContentItem(contentTypeDefinition.Name);
            
            if (contentItemVersionRecord != null)
            {
                contentItem.VersionRecord = contentItemVersionRecord;
            }
            
            foreach (var partRecord in contentParts)
            {
                var partRecordType = partRecord.GetType();
                foreach (var part in contentItem.Parts)
                {
                    if (this.partTypeRecordMatchingService.Match(part,partRecord))
                    {
                        this.partTypeRecordMatchingService.Set(part, partRecord);
                    }
                }
            }

            return contentItem;
        }
    }
}