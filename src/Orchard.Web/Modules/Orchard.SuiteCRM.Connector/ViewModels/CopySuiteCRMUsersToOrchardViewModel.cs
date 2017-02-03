using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class CopySuiteCRMUsersToOrchardViewModel
    {
        private List<SuiteCRMUserViewModel> users = new List<SuiteCRMUserViewModel>();

        public string DefaultPassword { get; set; }

        public List<SuiteCRMUserViewModel> Users
        {
            get
            {
                return this.users;
            }
        }
    }
}