using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Providers.Serialization;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Dashboard.Drivers
{
    public class SidebarDriver : ContentPartDriver<SidebarPart>
    {
        protected readonly ICRMContentOwnershipService crmContentOwnershipService;

        public SidebarDriver(ICRMContentOwnershipService crmContentOwnershipService)
        {
            this.crmContentOwnershipService = crmContentOwnershipService;
        }

        protected override DriverResult Display(SidebarPart part, string displayType, dynamic shapeHelper)
        {
            dynamic model = new ExpandoObject();
            model.Part = part;
            model.IsAdmin = this.crmContentOwnershipService.IsCurrentUserAdvanceOperator();
            return ContentShape("Parts_Sidebar", () => shapeHelper.Parts_Sidebar(Model: model));
        }
    }
}