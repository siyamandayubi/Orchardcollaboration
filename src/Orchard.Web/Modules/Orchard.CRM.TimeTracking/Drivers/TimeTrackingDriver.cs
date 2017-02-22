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
    public class TimeTrackingDriver : ContentPartDriver<TimeTrackingPart>
    {
        private readonly IOrchardServices services;
        protected readonly ICRMContentOwnershipService contentOwnershipService;
        private readonly ITimeTrackingService timeTrackingService;

        public TimeTrackingDriver(IOrchardServices services, ICRMContentOwnershipService contentOwnershipService, ITimeTrackingService timeTrackingService)
        {
            this.timeTrackingService = timeTrackingService;
            this.contentOwnershipService = contentOwnershipService;
            this.services = services;
        }

        protected override DriverResult Display(TimeTrackingPart part, string displayType, dynamic shapeHelper)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator() || !this.contentOwnershipService.CurrentUserCanViewContent(part.ContentItem))
            {
                return null;
            }

            if (displayType != "Detail")
            {
                return null;
            }

            var timeTrackingItems = this.timeTrackingService.GetTimeTrackingItems(part.Id);
            dynamic model = new ExpandoObject();
            var items = this.timeTrackingService.GetTimeTrackingItems(part.Id);
            int currentUserId = this.services.WorkContext.CurrentUser.Id;
            items.ToList().ForEach(c => c.UserCanEdit = this.contentOwnershipService.IsCurrentUserAdvanceOperator() || c.UserId == currentUserId);
            model.Part = part;
            model.Items = items;

            List<ContentShapeResult> returnValue = new List<ContentShapeResult>();

            returnValue.Add(ContentShape("Parts_TimeTrackingPart_Detail",
                () => shapeHelper.Parts_TimeTrackingPart_Detail(Model: model)));

            returnValue.Add(ContentShape("Parts_TimeTrackingPart_Header",
                () => shapeHelper.Parts_TimeTrackingPart_Header(Model: model)));

            return this.Combined(returnValue.ToArray());
        }

        protected override DriverResult Editor(TimeTrackingPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return null;
        }

        protected override DriverResult Editor(TimeTrackingPart part, dynamic shapeHelper)
        {
            return null;
        }
    }
}