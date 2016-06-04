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

        public DateTime? DueDate { get; set; }
        public ContentItemRecord RelatedContentItem { get; set; }

        [Aggregate]
        public virtual TicketPartRecord Parent { get; set; }

        public virtual UserPartRecord RequestingUser { get; set; }

        public virtual ServiceRecord Service { get; set; }

        public virtual int SourceId { get; set; }
        public virtual string SourceData { get; set; }

        public virtual TicketTypeRecord TicketType { get; set; }

        [CascadeAllDeleteOrphan, Aggregate]
        [XmlArray("SubTickets")]
        public virtual IList<TicketPartRecord> SubTickets { get; set; }

        public virtual TicketIdentityRecord Identity { get; set; }
    }
}