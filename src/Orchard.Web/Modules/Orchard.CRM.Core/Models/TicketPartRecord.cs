namespace Orchard.CRM.Core.Models
{
    using Orchard.ContentManagement.Records;
    using Orchard.Data.Conventions;
    using Orchard.Users.Models;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class TicketPartRecord : ContentPartRecord
    {
         public TicketPartRecord()
        {
            this.SourceId = TicketSourceTypes.CMS;
        }

        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual PriorityRecord PriorityRecord { get; set; }

        public virtual StatusRecord StatusRecord { get; set; }

        public virtual DateTime? DueDate { get; set; }
        public virtual ContentItemRecord RelatedContentItem { get; set; }

        [Aggregate]
        public virtual TicketPartRecord Parent { get; set; }

        public virtual UserPartRecord RequestingUser { get; set; }

        public virtual ServicePartRecord Service { get; set; }

        public virtual int SourceId { get; set; }
        public virtual string SourceData { get; set; }

        public virtual TicketTypeRecord TicketType { get; set; }

        public virtual DateTime? ClosedDateTime { get; set; }

        [CascadeAllDeleteOrphan, Aggregate]
        [XmlArray("SubTickets")]
        public virtual IList<TicketPartRecord> SubTickets { get; set; }

        public virtual TicketIdentityRecord Identity { get; set; }
    }
}