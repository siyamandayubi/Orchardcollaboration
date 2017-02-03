using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class TeamMemberPartRecord : ContentPartRecord
    {
        public virtual TeamPartRecord TeamPartRecord { get; set; }

        [Aggregate]
        public virtual UserPartRecord UserPartRecord { get; set; }
    }
}