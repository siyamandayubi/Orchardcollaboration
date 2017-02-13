using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class AttachToFolderListViewModel
    {
        private Collection<dynamic> items = new Collection<dynamic>();

        public Collection<dynamic> Items { get { return this.items; } }
        public dynamic Pager { get; set; }
    }
}