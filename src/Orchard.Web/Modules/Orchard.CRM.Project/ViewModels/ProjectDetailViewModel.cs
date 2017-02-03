using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class ProjectDetailViewModel
    {
        public dynamic MenuShape { get; set; }
        public ProjectPart ProjectPart { get; set; }
        public dynamic Detail { get; set; }
        public bool CurrentUserCanChangePermission { get; set; }
        public bool CurrentUserCanEdit { get; set; }
    }
}