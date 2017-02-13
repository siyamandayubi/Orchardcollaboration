using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class ProjectListViewModel
    {
        private Collection<dynamic> projects = new Collection<dynamic>();

        public Collection<dynamic> Projects { get { return this.projects; } }
        public bool CanCreateProject { get; set; }
        public dynamic Pager { get; set; }
    }
}