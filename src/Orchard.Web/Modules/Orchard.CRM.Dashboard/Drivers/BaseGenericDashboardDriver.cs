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
using Orchard.CRM.Core.Services;
using Orchard.Core.Title.Models;
using Orchard.CRM.Dashboard.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System;

namespace Orchard.CRM.Dashboard.Drivers
{
    public abstract class BaseGenericDashboardDriver<TContentPart> : ContentPartDriver<TContentPart>
        where TContentPart : ContentPart, new()
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly IOrchardServices _orchardServices;
        protected readonly IContentManager _contentManager;
        protected readonly ISiteService _siteService;
        protected readonly IFeedManager _feedManager;
        protected readonly IContainerService _containerService;
        protected readonly ICRMContentOwnershipService _crmContentOwnershipService;
        protected int pageSize = 10;
        public BaseGenericDashboardDriver(
            ICRMContentOwnershipService crmContentOwnershipService,
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            ISiteService siteService,
            IFeedManager feedManager, IContainerService containerService)
        {
            _crmContentOwnershipService = crmContentOwnershipService;
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _contentManager = orchardServices.ContentManager;
            _siteService = siteService;
            _feedManager = feedManager;
            _containerService = containerService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(TContentPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType != "Detail")
            {
                return null;
            }

            dynamic listShape = null;
            Func<dynamic> loadItems = () =>
            {
                var container = part.ContentItem;
                var query = _contentManager
                .Query(VersionOptions.Published)
                .Join<CommonPartRecord>().Where(x => x.Container.Id == container.Id)
                .Join<ContainablePartRecord>().OrderBy(x => x.Position);

                var startIndex = 0;
                var pageOfItems = pageSize > 0 ? query.Slice(startIndex, pageSize).ToList() : query.List();

                listShape = shapeHelper.List();
                listShape.AddRange(pageOfItems.Select(item => _contentManager.BuildDisplay(item, "Summary")));
                listShape.Classes.Add("content-items");

                return listShape;
            };

            bool isAdmin = _crmContentOwnershipService.IsCurrentUserAdvanceOperator();

            return Combined(ContentShape("Parts_GenericDashboard_Detail", () =>
            {
                // load items one time per ContentShape
                listShape = listShape ?? loadItems();

                listShape.Classes.Add("list-items");
                return shapeHelper.Parts_GenericDashboard_Detail(
                  List: listShape,
                  isAdmin: isAdmin,
                      Pager: null);
            }),
            ContentShape("Parts_GenericDashboard_Header", () =>
            {
                // load items one time per ContentShape
                listShape = listShape ?? loadItems();

                return Header(part, listShape, shapeHelper);
            }));
        }

        protected virtual dynamic Header(TContentPart part, dynamic listShape, dynamic shapeHelper)
        {
            return null;
        }

        protected override DriverResult Editor(TContentPart part, dynamic shapeHelper)
        {
            if (!_crmContentOwnershipService.IsCurrentUserAdvanceOperator())
            {
                return null;
            }

            var currentPortlets = GetCurrentPortlets(part).ToList();

            var portlets = _orchardServices
                .ContentManager
                .HqlQuery()
                .ForType(GetPortletTypes())
                .List()
                .Select(c => c.As<TitlePart>())
                .ToList();

            List<EditDashboardViewModel> model = new List<EditDashboardViewModel>();


            // we assume all portlets have TitleParts
            foreach (var item in portlets.Where(c => c.Is<TitlePart>()))
            {
                string title = string.IsNullOrEmpty(item.Title) ? string.Empty : item.Title.ToUpper(CultureInfo.InvariantCulture).Trim();
                EditDashboardViewModel modelMember = new EditDashboardViewModel();
                modelMember.PortletId = item.Id;
                modelMember.Title = item.As<TitlePart>().Title;
                modelMember.IsChecked = currentPortlets.Contains(title);
                modelMember.Order = modelMember.IsChecked ? currentPortlets.IndexOf(item.Title) : -int.MaxValue;

                model.Add(modelMember);
            }

            model = model.OrderByDescending(c => c.Order).ToList();

            return ContentShape(GetEditorShapeType(),
                        () => shapeHelper.EditorTemplate(
                            TemplateName: GetEditorShapeAddress(),
                            Model: model,
                            Prefix: Prefix));
        }

        protected abstract string[] GetPortletTypes();
        public abstract string GetEditorShapeType();
        public abstract string GetEditorShapeAddress();
        protected abstract string[] GetCurrentPortlets(TContentPart part);
    }
}