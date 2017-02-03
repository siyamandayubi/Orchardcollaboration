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