using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class EditMilestoneViewModel
    {
        public virtual DateTime? Start { get; set; }
        public virtual DateTime? End { get; set; }
        public virtual bool IsCurrent { get; set; }
        public virtual bool IsBacklog { get; set; }
        public virtual bool IsClosed { get; set; }
    }
}