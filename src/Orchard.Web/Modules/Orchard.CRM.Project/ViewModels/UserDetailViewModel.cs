using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class UserDetailViewModel : UserViewModel
    {
        public dynamic ActivityStream { get; set; }
        public bool IsOperator { get; set; }
        public bool IsCustomer { get; set; }
    }
}