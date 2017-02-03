using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class EditPortletViewModel
    {
        public string Title { get; set; }
        public int PortletId { get; set; }
        public int Order { get; set; }
        public bool IsChecked { get; set; }
    }
}