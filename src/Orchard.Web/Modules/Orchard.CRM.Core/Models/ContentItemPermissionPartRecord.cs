using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System.Collections.Generic;

namespace Orchard.CRM.Core.Models
{
    public class ContentItemPermissionPartRecord : ContentPartRecord
    {
        public virtual TicketPartRecord Ticket { get; set; }

        public virtual bool HasOwner { get; set; }

        [Aggregate]
        public virtual IList<ContentItemPermissionDetailRecord> Items { get; set; }
    }
}