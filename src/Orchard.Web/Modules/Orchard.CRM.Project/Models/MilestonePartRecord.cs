using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Models
{
    public class MilestonePartRecord : ContentPartRecord
    {
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public virtual bool IsCurrent { get; set; }
        public virtual bool IsBacklog { get; set; }
        public virtual bool IsClosed { get; set; }
    }
}