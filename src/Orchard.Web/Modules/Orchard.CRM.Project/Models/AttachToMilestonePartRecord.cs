using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Models
{
    public class AttachToMilestonePartRecord : ContentPartRecord
    {
        public virtual int? MilestoneId { get; set; }
        public virtual int OrderId { get; set; }
        public virtual int? Size { get; set; }
    }
}