
using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class MainViewModel
    {
        private List<SuiteCRMProjectDetailViewModel> projects = new List<SuiteCRMProjectDetailViewModel>();
        private List<SuiteCRMUserViewModel> users = new List<SuiteCRMUserViewModel>();

        public List<SuiteCRMUserViewModel> Users { get { return this.users; } }
        public List<SuiteCRMProjectDetailViewModel> Projects { get { return this.projects; } }

        public bool ViewUsersPage { get; set; }
        public int SuiteCRMUsersCount { get; set; }
        public int PageSize { get; set; }
        public int SuiteCRMProjectsCount { get; set; }
        public int OrchardCollaborationProjectsCount { get; set; }
        public int SuiteCRMPage { get; set; }
        public int OrchardCollaborationPage { get;set; }
        public bool ListedBasedOnSuiteCRM { get; set; }

        public dynamic TranslateTable { get; set; }
        public dynamic Routes { get; set; }
    }
}