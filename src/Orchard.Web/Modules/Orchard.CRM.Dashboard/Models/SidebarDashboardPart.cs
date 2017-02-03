using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Core.Containers.Models;

namespace Orchard.CRM.Dashboard.Models
{
    public class SidebarDashboardPart : ContentPart
    {
        /// <summary>
        /// Comma-seperated list of the Portlet titles for the given dashboard
        /// </summary>
        public string SidebarPortletList
        {
            get { return this.Retrieve(x => x.SidebarPortletList); }
            set { this.Store(x => x.SidebarPortletList, value); }
        }
    }
}