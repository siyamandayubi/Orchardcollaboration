using Orchard.ContentManagement.Drivers;
using Orchard.CRM.TimeTracking.Models;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Services;
using Orchard.CRM.TimeTracking.Services;
using System.Dynamic;
using System.Linq;
using System.Collections.Generic;

namespace Orchard.CRM.TimeTracking.Drivers
{
    public class TimeTrackingItemDriver : ContentPartDriver<TimeTrackingItemPart>
    {
        private readonly IOrchardServices services;
        protected readonly ICRMContentOwnershipService contentOwnershipService;
        private readonly ITimeTrackingService timeTrackingService;

        public TimeTrackingItemDriver(IOrchardServices services, ICRMContentOwnershipService contentOwnershipService, ITimeTrackingService timeTrackingService)
        {
            this.timeTrackingService = timeTrackingService;
            this.contentOwnershipService = contentOwnershipService;
            this.services = services;
        }

        protected override DriverResult Display(TimeTrackingItemPart part, string displayType, dynamic shapeHelper)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator() || !this.contentOwnershipService.CurrentUserCanViewContent(part.ContentItem))
            {
                return null;
            }

            return ContentShape("Parts_TimeTrackingItemPart_Detail",
                () => shapeHelper.Parts_TimeTrackingItemPart_Detail(Model: part));
        }

        protected override DriverResult Editor(TimeTrackingItemPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return null;
        }

        protected override DriverResult Editor(TimeTrackingItemPart part, dynamic shapeHelper)
        {
            return null;
        }
    }
}