using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Core.Feeds;
using Orchard.Localization;
using Orchard.Settings;
using System.Linq;
using System.Web.Routing;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Dashboard.Models;
using Orchard.Core.Title.Models;
using System.Web.Mvc;
using System.Globalization;
using System.Dynamic;
using Orchard.CRM.Dashboard.ViewModels;
using System.Collections.Generic;
using Orchard.CRM.Core.Services;

namespace Orchard.CRM.Dashboard.Drivers
{
    public class SidebarDashboardDriver : BaseGenericDashboardDriver<SidebarDashboardPart>
    {
        public SidebarDashboardDriver(
            ICRMContentOwnershipService crmContentOwnershipService,
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            ISiteService siteService,
            IFeedManager feedManager, IContainerService containerService)
            : base(crmContentOwnershipService, contentDefinitionManager, orchardServices, siteService, feedManager, containerService)
        {
        }

        protected override DriverResult Editor(SidebarDashboardPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (!_crmContentOwnershipService.IsCurrentUserAdvanceOperator())
            {
                return null;
            }

            List<EditDashboardViewModel> model = new List<EditDashboardViewModel>();
            updater.TryUpdateModel(model, "Portlets", null, null);

            var selectedPortlets = model.Where(c => c.IsChecked).OrderBy(c => c.Order).Select(c => c.PortletId).ToList();
            var portlets = _orchardServices.ContentManager.GetMany<TitlePart>(selectedPortlets, VersionOptions.Published, QueryHints.Empty);
            part.SidebarPortletList = string.Join(",", portlets.Select(c => c.Title));

            return Editor(part, shapeHelper);
        }

        protected override string[] GetCurrentPortlets(SidebarDashboardPart part)
        {
            return string.IsNullOrEmpty(part.SidebarPortletList) ?
                   new string[]{}:
                   part.SidebarPortletList.Split(',').Select(c => c.ToUpper(CultureInfo.InvariantCulture).Trim()).ToArray();           
        }

        protected override string[] GetPortletTypes()
        {
            return new[] {Consts.SidebarProjectionPortletTemplateType, Consts.SidebarStaticPortletType};
        }

        public override string GetEditorShapeType()
        {
            return "Parts_SidebarDashboard_Edit";
        }

        public override string GetEditorShapeAddress()
        {
            return "Parts/SidebarDashboard";
        }
    }
}