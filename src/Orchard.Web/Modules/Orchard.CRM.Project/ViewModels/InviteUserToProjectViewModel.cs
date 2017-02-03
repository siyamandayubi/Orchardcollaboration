using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.ViewModels
{
    public class InviteUserToProjectViewModel
    {
        private List<SelectListItem> projects = new List<SelectListItem>();
        
        public UserViewModel User { get; set; }

        public List<SelectListItem> Projects
        {
            get
            {
                return this.projects;
            }
        }
    }
}