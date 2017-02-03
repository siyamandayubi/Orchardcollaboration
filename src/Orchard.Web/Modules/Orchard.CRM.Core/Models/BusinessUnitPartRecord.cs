using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Orchard.CRM.Core.Models
{
    public class BusinessUnitPartRecord : ContentPartRecord, IBasicDataRecord
    {
        public virtual BusinessUnitPartRecord Parent { get; set; }
        public virtual string Description { get; set; }
        public virtual string Name { get; set; }

        [Aggregate]
        [XmlArray("Teams")]
        public virtual IList<TeamPartRecord> Teams { get; set; }

        [Aggregate]
        [XmlArray("Childs")]
        public virtual IList<BusinessUnitPartRecord> Childs { get; set; }

        [Aggregate]
        [XmlArray("BusinessUnitMembers")]
        public virtual IList<BusinessUnitMemberPartRecord> Members { get; set; }
    }
}