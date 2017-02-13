using Orchard.ContentManagement.Drivers;
using Orchard.Logging;
using Orchard.SuiteCRM.Connector.Models;
using Orchard.SuiteCRM.Connector.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Drivers
{
    public class SuiteCRMStatusNotificationDriver : ContentPartDriver<SuiteCRMStatusNotificationPart>
    {
        private readonly IOrchardServices services;

        public SuiteCRMStatusNotificationDriver(IOrchardServices services)
        {
            this.services = services;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        protected override DriverResult Display(SuiteCRMStatusNotificationPart part, string displayType, dynamic shapeHelper)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageSuiteCRMPermission))
            {
                return null;
            }

            bool isConnectionAvailable = Helper.IsDatabaseConnectionProvided(services, Logger);

            if (isConnectionAvailable)
            {
                return null;
            }

            return ContentShape("Parts_SuiteCRMStatusNotification", () => shapeHelper.Parts_SuiteCRMStatusNotification(Model: part));
        }
    }
}