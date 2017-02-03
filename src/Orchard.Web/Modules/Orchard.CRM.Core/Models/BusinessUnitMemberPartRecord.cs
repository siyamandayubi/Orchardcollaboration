using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class BusinessUnitMemberPartRecord : ContentPartRecord
    {
        public virtual BusinessUnitPartRecord BusinessUnitPartRecord { get; set; }
       
        [Aggregate]
        public virtual UserPartRecord UserPartRecord { get; set; }
    }
}