using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.ActivityStream
{
    public class ActivityStreamChangeItem
    {
        public ActivityStreamChangeItem(string change)
        {
            this.Change = change;
        }

        public ActivityStreamChangeItem(string change, bool requireNewRecord, int? relatedContentId, int? versionId)
        {
            this.RelatedContentVersionId = versionId;
            this.RelatedContentId = relatedContentId;
            this.Change = change;
            this.RequireNewRecord = requireNewRecord;
        }

        public int? RelatedContentVersionId { get; private set; }
        public int? RelatedContentId { get; private set; }
        public string Change { get; private set; }
        public bool RequireNewRecord { get; private set; }
    }
}