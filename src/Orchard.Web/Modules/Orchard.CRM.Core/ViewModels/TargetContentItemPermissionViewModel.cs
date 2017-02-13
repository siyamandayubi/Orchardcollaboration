using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class TargetContentItemPermissionViewModel
    {
        public int? BusinessUnitId { get; set; }
        public int? TeamId { get; set; }
        public int? UserId { get; set; }
        public bool Checked { get; set; }
    }
}