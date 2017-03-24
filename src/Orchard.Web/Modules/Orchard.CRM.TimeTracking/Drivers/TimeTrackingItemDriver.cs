using Orchard.ContentManagement.Drivers;
using Orchard.CRM.TimeTracking.Models;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Services;
using Orchard.CRM.TimeTracking.Services;
using System.Dynamic;
using System.Linq;
using System.Collections.Generic;
using Orchard.Security;

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

            var user = this.services.ContentManager.Get<IUser>(part.Record.User.Id);

            dynamic model = new ExpandoObject();
            model.Comment = part.Record.Comment;
            model.OriginalTimeTrackingString = part.Record.OriginalTimeTrackingString;
            model.TrackingDate = part.Record.TrackingDate;
            model.FullUsername = user !=null? CRMHelper.GetFullNameOfUser(user):string.Empty;
            return Combined(
                ContentShape("Parts_TimeTrackingItemPart_Detail",
                        () => shapeHelper.Parts_TimeTrackingItemPart_Detail(Model: model)),
                ContentShape("Parts_TimeTrackingItemPart_TableRow",
                        () => shapeHelper.Parts_TimeTrackingItemPart_TableRow(Model: model)),
                ContentShape("Parts_TimeTrackingItemPart_Summary",
                        () => shapeHelper.Parts_TimeTrackingItemPart_Summary(Model: model))
                );
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