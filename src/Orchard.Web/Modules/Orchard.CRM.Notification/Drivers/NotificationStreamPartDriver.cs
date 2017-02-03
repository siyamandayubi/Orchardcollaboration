namespace Orchard.CRM.Notification.Drivers
{
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Drivers;
    using Orchard.Core.Title.Models;
    using Orchard.CRM.Core.Drivers;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.Settings;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.CRM.Notification.Models;
    using Orchard.CRM.Notification.Services;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Projections.Models;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class NotificationStreamPartDriver : CRMContentPartDriver<NotificationStreamPart>
    {
        private readonly IActivityNotificationService notificationStreamService;
        private readonly IActivityStreamService activityStreamService;
        private readonly IBasicDataService basicDataService;
        private readonly IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort;
        private readonly Lazy<ISessionLocator> sessionLocator;

        public NotificationStreamPartDriver(
            IActivityStreamService activityStreamService,
            IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort,
            IOrchardServices services,
            Lazy<ISessionLocator> sessionLocator,
            IBasicDataService basicDataService,
            IActivityNotificationService notificationStreamService)
            : base(services)
        {
            this.activityStreamService = activityStreamService;
            this.projectionManagerWithDynamicSort = projectionManagerWithDynamicSort;
            this.basicDataService = basicDataService;
            this.notificationStreamService = notificationStreamService;
            this.sessionLocator = sessionLocator;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(NotificationStreamPart part, string displayType, dynamic shapeHelper)
        {
            if (this.services.WorkContext.CurrentUser == null)
            {
                return null;
            }

            // retrieving paging parameters
            var queryString = this.services.WorkContext.HttpContext.Request.QueryString;

            var settings = part.TypePartDefinition.Settings.GetModel<ActivityStreamPartSettings>();

            var pageKey = "page";
            var page = 0;


            // don't try to page if not necessary
            if (queryString.AllKeys.Contains(pageKey))
            {
                int.TryParse(queryString[pageKey], out page);
            }

            // At the moment, we don't show pagination, we show the first page including 100 items.
            // Due to updatingUserContentVisitRecord after loading the first page, pagination doesn't work
            int pageSize = 100;
            page = 0;

            int count = this.notificationStreamService.NewItemsCount();
            List<ActivityStreamRecord> items = this.notificationStreamService.Notifications(page == 0 ? page : page - 1, pageSize).ToList();

            var model = this.activityStreamService.CreateModel(items, count, page, pageSize);

            return this.ContentShape("Parts_NotificationStreamPart",
                () => shapeHelper.Parts_NotificationStreamPart(Model: model));
        }
    }
}