using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Dashboard.Models
{
    public class BasicDataPortletPart : ContentPart
    {
    }

    public class NavigationPortletPart : ContentPart
    {
    }

    public class WorkflowPortletPart : ContentPart
    {
    }

    public class QueriesPortletPart : ContentPart
    {
    }

    public class ContentManagementPortletPart : ContentPart
    {
    }

    public class DashboardManagementPart : ContentPart
    {
        public int DashboardListContentId
        {
            get { return this.Retrieve(x => x.DashboardListContentId); }
            set { this.Store(x => x.DashboardListContentId, value); }
        }

    }
}