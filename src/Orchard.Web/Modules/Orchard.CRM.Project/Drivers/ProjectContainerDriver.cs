using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Core.Feeds;
using Orchard.CRM.Project.Models;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Orchard.CRM.Project.Drivers
{
    public class ProjectContainerDriver : ContentPartDriver<ProjectContainerPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IFeedManager _feedManager;
        private readonly IContainerService _containerService;

        public ProjectContainerDriver(
            IContentDefinitionManager contentDefinitionManager, 
            IOrchardServices orchardServices, 
            ISiteService siteService,
            IFeedManager feedManager, IContainerService containerService) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _contentManager = orchardServices.ContentManager;
            _siteService = siteService;
            _feedManager = feedManager;
            _containerService = containerService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(ProjectContainerPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Container_Contained", () =>
            {
                var container = part.ContentItem;
                var query = _contentManager
                .Query(VersionOptions.Published)
                .Join<CommonPartRecord>().Where(x => x.Container.Id == container.Id)
                .Join<ContainablePartRecord>().OrderByDescending(x => x.Position);

                var metadata = container.ContentManager.GetItemMetadata(container);
                if (metadata != null)
                    _feedManager.Register(metadata.DisplayText, "rss", new RouteValueDictionary { { "containerid", container.Id } });

                var startIndex =  0;
                var pageOfItems = query.Slice(startIndex, 10).ToList();

                var listShape = shapeHelper.List();
                listShape.AddRange(pageOfItems.Select(item => _contentManager.BuildDisplay(item, "Summary")));
                listShape.Classes.Add("content-items");
                listShape.Classes.Add("list-items");

                return shapeHelper.Parts_Container_Contained(
                    List: listShape,
                    Pager: null
                );
            });
        }
    }
}