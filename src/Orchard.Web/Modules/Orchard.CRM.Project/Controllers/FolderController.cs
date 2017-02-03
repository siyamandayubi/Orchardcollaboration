using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.CRM.Core;
using Orchard.CRM.Core.Controllers;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using Orchard.Settings;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.Controllers
{
    [Themed()]
    [ValidateInput(false)]
    public class FolderController : BaseController
    {
        public FolderController(
            IIndexProvider indexProvider,
            IContentOwnershipHelper contentOwnershipHelper,
            ICRMContentOwnershipService crmContentOwnershipService,
            IExtendedContentManager extendedContentManager,
            ITransactionManager transactionManager,
            IWidgetService widgetService,
            IThemeAwareViewEngine themeAwareViewEngine,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IOrchardServices services,
            IDisplayHelperFactory displayHelperFactory,
            IBusinessUnitService businessUnitService,
            ISiteService siteService,
            IBasicDataService basicDataService,
            IContentDefinitionManager contentDefinitionManager,
            IIndexManager indexManager,
            IWorkContextAccessor workContextAccessor,
            IActivityStreamService activityStreamService,
            IContentItemDescriptorManager contentItemDescriptorManager)
            : base(ContentTypes.FolderContentType, "Folder_Edit", indexProvider, services, crmContentOwnershipService, transactionManager, extendedContentManager, contentManager, widgetService, themeAwareViewEngine, shapeFactory, displayHelperFactory, basicDataService, contentOwnershipHelper, activityStreamService, contentItemDescriptorManager)
        {
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override bool IsCreateAuthorized()
        {
            return this.crmContentOwnershipService.IsCurrentUserOperator();
        }

        public override ActionResult Remove(int[] ids, string returnUrl)
        {
            return base.Remove(ids, returnUrl);
        }

        protected override bool IsDisplayAuthorized()
        {
            return this.services.Authorizer.Authorize(Permissions.OperatorPermission) ||
                this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission) ||
                this.services.Authorizer.Authorize(Permissions.CustomerPermission);
        }
    }
}