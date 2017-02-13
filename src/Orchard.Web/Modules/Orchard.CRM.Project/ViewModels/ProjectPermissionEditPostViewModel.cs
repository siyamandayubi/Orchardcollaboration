using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class ProjectPermissionEditPostViewModel
    {
        public bool VisibleToAll { get; set; }

        [Required]
        public int? ProjectId { get; set; }
        public int[] Users { get; set; }
        public int[] Customers { get; set; }
        public int[] BusinessUnits { get; set; }
        public int[] Teams { get; set; }
    }
}