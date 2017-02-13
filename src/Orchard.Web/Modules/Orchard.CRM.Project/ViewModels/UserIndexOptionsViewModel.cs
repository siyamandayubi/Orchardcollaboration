using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class UserIndexOptionsViewModel
    {
        public string Search { get; set; }
        public UsersOrderViewModel Order { get; set; }
        public bool OrderByDescending { get; set; }
    }

}