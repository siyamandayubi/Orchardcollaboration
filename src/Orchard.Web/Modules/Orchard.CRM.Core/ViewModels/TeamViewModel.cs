using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class TeamViewModel
    {
        private Collection<UserPart> users = new Collection<UserPart>();

        public Collection<UserPart> Users
        {
            get
            {
                return this.users;
            }
        }

        public int TeamId { get; set; }
        public int? BusinessUnitId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Checked { get; set; }
    }
}