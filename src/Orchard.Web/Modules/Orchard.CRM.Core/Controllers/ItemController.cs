using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.CRM.Core.Models;
using Orchard.Indexing;
using Orchard.Themes;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;

namespace Orchard.CRM.Core.Controllers
{
    [Themed]
    [ValidateInput(false)]
    public class ItemController : BaseController
    {
        public ItemController(
            IIndexProvider indexProvider,
            ICRMContentOwnershipService crmContentOwnershipService,
            IContentOwnershipHelper contentOwnershipHelper,
            IOrchardServices services,
            IExtendedContentManager extendedContentManager,
            ITransactionManager transactionManager,
            IWidgetService widgetService,
            IThemeAwareViewEngine themeAwareViewEngine,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IBasicDataService basicDataService,
            IDisplayHelperFactory displayHelperFactory,
            IActivityStreamService streamService,
            IContentItemDescriptorManager contentItemDescriptorManager)
            : base(string.Empty, string.Empty, indexProvider, services, crmContentOwnershipService, transactionManager, extendedContentManager, contentManager, widgetService, themeAwareViewEngine, shapeFactory, displayHelperFactory, basicDataService, contentOwnershipHelper, streamService, contentItemDescriptorManager)
        {
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override bool IsCreateAuthorized()
        {
            return this.crmContentOwnershipService.IsCurrentUserOperator() || crmContentOwnershipService.IsCurrentUserCustomer();
        }

        protected override bool IsDisplayAuthorized()
        {
            return this.crmContentOwnershipService.IsCurrentUserOperator() || crmContentOwnershipService.IsCurrentUserCustomer();
        }
     }
}