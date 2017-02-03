using Orchard.ContentManagement.MetaData;
using Orchard.Core.Containers.Services;
using Orchard.Core.Feeds;
using Orchard.Settings;
using Orchard.CRM.Dashboard.Models;
using Orchard.CRM.Core.Services;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Dashboard.Services;

namespace Orchard.CRM.Dashboard.Drivers
{
    public class GenericDashboardDriver : BaseGenericDashboardDriver<GenericDashboardPart>
    {

        public GenericDashboardDriver(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            ICRMContentOwnershipService crmContentOwnershipService,
            ISiteService siteService,
            IFeedManager feedManager, IContainerService containerService)
            : base(crmContentOwnershipService, contentDefinitionManager, orchardServices, siteService, feedManager, containerService)
        {
            // display all  the items in the Dashboard
            pageSize = -1;
        }

        protected override string[] GetCurrentPortlets(GenericDashboardPart part)
        {
            return string.IsNullOrEmpty(part.PortletList) ?
                   new string[] { } :
                   part.PortletList.Split(',').Select(c => c.ToUpper(CultureInfo.InvariantCulture).Trim()).ToArray();
        }

        protected override string[] GetPortletTypes()
        {
            return new[] { Consts.GenericProjectionPortletTemplateType, Consts.GenericReportViewerPortletTemplateType, Consts.GenericActivityStreamPortletTemplateType };
        }

        protected override dynamic Header(GenericDashboardPart part, dynamic listShape, dynamic shapeHelper)
        {
            bool isAdmin = _crmContentOwnershipService.IsCurrentUserAdvanceOperator();

            if (!isAdmin)
            {
                return null;
            }

            int toConfigureItemsCount = 0;
            foreach (var item in listShape.Items)
            {
                if (item.Content != null &&
                    item.Content.Items.Count > 0 &&
                    item.Content.Items[0].Model != null &&
                    item.Content.Items[0].Model.IsConfigured != null &&
                    item.Content.Items[0].Model.IsConfigured == false)
                {
                    toConfigureItemsCount++;
                }
            }

            return shapeHelper.Parts_GenericDashboard_Header(
              ToConfigureItemsCount: toConfigureItemsCount);
        }
        public override string GetEditorShapeType()
        {
            return "Parts_GenericDashboard_Edit";
        }

        public override string GetEditorShapeAddress()
        {
            return "Parts/GenericDashboard";
        }
    }
}