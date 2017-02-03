namespace Orchard.CRM.Core.Models
{
    using Orchard.ContentManagement.Records;
    using Orchard.Users.Models;
    using System;

    public class ActivityStreamRecord
    {
        public virtual int Id { get; set; }
        public virtual ContentItemRecord RelatedContent { get; set; }
        public virtual ContentItemVersionRecord RelatedVersion { get; set; }
        public virtual string Description { get; set; }
        public virtual UserPartRecord User { get; set; }
        public DateTime CreationDateTime { get; set; }
    }
}