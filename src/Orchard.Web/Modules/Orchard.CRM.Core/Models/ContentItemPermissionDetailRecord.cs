using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class ContentItemPermissionDetailRecord
    {
        public virtual int Id { get; set; }
        public virtual TeamPartRecord Team { get; set; }
        public virtual BusinessUnitPartRecord BusinessUnit { get; set; }
        public virtual UserPartRecord User { get; set; }
        public virtual ContentItemPermissionPartRecord ContentItemPermissionPartRecord { get; set; }
        public virtual ContentItemPermissionDetailRecord Parent { get; set; }
        public virtual byte AccessType { get; set; }
    }
}