using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class UserListViewModel
    {
        private Collection<UserViewModel> users = new Collection<UserViewModel>();

        public Collection<UserViewModel> Users
        {
            get { return this.users; }
        }

        public dynamic Pager { get; set; }
        public string Search { get; set; }
        public UsersOrderViewModel OrderBy { get; set; }
        public bool OrderByDescending { get; set; }
    }
}