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
    public class SuiteCRMUserDriver : ContentPartDriver<SuiteCRMUserPart>
    {
        private readonly IOrchardServices services;

        public SuiteCRMUserDriver(IOrchardServices services)
        {
            this.services = services;
            Logger = NullLogger.Instance;
        }
       
        public ILogger Logger { get; set; }

        protected override DriverResult Display(SuiteCRMUserPart part, string displayType, dynamic shapeHelper)
        {
            return null;
        }

        protected override DriverResult Editor(SuiteCRMUserPart part, ContentManagement.IUpdateModel updater, dynamic shapeHelper)
        {
            return null;
        }

        protected override DriverResult Editor(SuiteCRMUserPart part, dynamic shapeHelper)
        {
            return null;
        }
    }
}