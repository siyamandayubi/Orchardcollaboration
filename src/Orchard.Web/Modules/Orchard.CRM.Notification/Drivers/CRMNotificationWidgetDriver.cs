using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Notification.Models;
using Orchard.CRM.Notification.Services;
using System.Dynamic;

namespace Orchard.CRM.Notification.Drivers
{
    public class CRMNotificationWidgetDriver : ContentPartDriver<CRMNotificationWidgetPart>
    {
        private readonly IActivityNotificationService activityNotificationService;
        private readonly IOrchardServices services;
        public CRMNotificationWidgetDriver(IActivityNotificationService activityNotificationService, IOrchardServices services)
        {
            this.services = services;
            this.activityNotificationService = activityNotificationService;
        }

        protected override DriverResult Display(CRMNotificationWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var contentItem = activityNotificationService.GetLatestCRMNotificationListItem();

            if (contentItem == null)
            {
                return null;
            }

            var user = this.services.WorkContext.CurrentUser;

            if (user == null)
            {
                return null;
            }

            dynamic model = new ExpandoObject();
            model.Part = part;
            model.UserId = user.Id;
            model.ListContentItemId = contentItem.Id;

            return ContentShape("Parts_CRMNotification",
                 () => shapeHelper.Parts_CRMNotification(
                     Model: model));
        }
    }
}