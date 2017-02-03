using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.CRM.Dashboard.Models;
using S22.IMAP.Models;
using Orchard.ContentManagement;
using Newtonsoft.Json.Linq;

namespace Orchard.CRM.Dashboard.Drivers
{
    public class BasicDataPortletDriver : ContentPartDriver<BasicDataPortletPart>
    {
        protected override DriverResult Display(BasicDataPortletPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_BasicDataPortletPart_Summary", () => shapeHelper.Parts_BasicDataPortletPart_Summary(Model: part));
        }
    }

    public class NavigationPortletDriver : ContentPartDriver<NavigationPortletPart>
    {
        protected readonly IOrchardServices orchardServices;

        protected override DriverResult Display(NavigationPortletPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_NavigationPortletPart_Summary", () => shapeHelper.Parts_NavigationPortletPart_Summary(Model: part));
        }
    }

    public class WorkflowPortletDriver : ContentPartDriver<WorkflowPortletPart>
    {
        protected override DriverResult Display(WorkflowPortletPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_WorkflowPortletPart_Summary", () => shapeHelper.Parts_WorkflowPortletPart_Summary(Model: part));
        }
    }

    public class QueriesPortletDriver : ContentPartDriver<QueriesPortletPart>
    {
        protected override DriverResult Display(QueriesPortletPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_QueriesPortletPart_Summary", () => shapeHelper.Parts_QueriesPortletPart_Summary(Model: part));
        }
    }

    public class ContentManagementPortletDriver : ContentPartDriver<ContentManagementPortletPart>
    {
        protected override DriverResult Display(ContentManagementPortletPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ContentManagementPortletPart_Summary", () => shapeHelper.Parts_ContentManagementPortletPart_Summary(Model: part));
        }
    }
}