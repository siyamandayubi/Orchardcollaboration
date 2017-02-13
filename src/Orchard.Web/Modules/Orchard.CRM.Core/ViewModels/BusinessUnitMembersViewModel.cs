using Orchard.CRM.Core.Models;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class BusinessUnitMembersViewModel
    {
        private Collection<CheckableViewModel<UserPart>> users = new Collection<CheckableViewModel<UserPart>>();

        public Collection<CheckableViewModel<UserPart>> Users
        {
            get
            {
                return this.users;
            }
        }

        public BusinessUnitViewModel BusinessUnit { get; set; }
    }
}