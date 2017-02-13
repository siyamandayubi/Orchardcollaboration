using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Orchard.CRM.Core.Models
{
    public class TeamPartRecord : ContentPartRecord
    {
        public virtual BusinessUnitPartRecord BusinessUnitPartRecord { get; set; }
        public virtual string Description { get; set; }
        public virtual string Name { get; set; }
   
        [CascadeAllDeleteOrphan, Aggregate]
        [XmlArray("TeamMembers")]
        public virtual IList<TeamMemberPartRecord> TeamMembers { get; set; }
    }
}