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
    public class CoverWidgetDriver : ContentPartDriver<CoverWidgetPart>
    {
        protected readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly IOrchardServices services;

        public CoverWidgetDriver(ICRMContentOwnershipService crmContentOwnershipService, IOrchardServices services)
        {
            this.services = services;
            this.crmContentOwnershipService = crmContentOwnershipService;
        }

        protected override DriverResult Display(CoverWidgetPart part, string displayType, dynamic shapeHelper)
        {
            dynamic model = new ExpandoObject();
            model.Part = part;
            model.IsAdmin = this.crmContentOwnershipService.IsCurrentUserAdvanceOperator();
            model.TargetContent = null;

            List<DriverResult> returnValue = new List<DriverResult>();
            if (part.LoadSync)
            {
                var targetContentItem = services.ContentManager.Get(part.TargetContentItemId);
                if (targetContentItem != null)
                {
                    model.TargetContent = services.ContentManager.BuildDisplay(targetContentItem, displayType: "Detail");

                    if (!part.HideEditLinkInFrontendLoadSync)
                    {
                        returnValue.Add(ContentShape("Parts_CoverWidget_EditLink", () =>
                            shapeHelper.Parts_CoverWidget_EditLink(model)));
                    }
                }
            }

            returnValue.Add(ContentShape("Parts_CoverWidget", () => shapeHelper.Parts_CoverWidget(Model: model)));

            return Combined(returnValue.ToArray());
        }
    }
}