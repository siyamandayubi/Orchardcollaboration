using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Core.Containers.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Web.Routing;
using Orchard.Settings;
using Orchard.Core.Feeds;
using Orchard.UI.Navigation;
using Orchard.CRM.Core.Models;
using Orchard.DisplayManagement;
using System.IO;
namespace Orchard.CRM.Project.Drivers
{
    public class ExtendedContainerDriver: ContentPartDriver<ContainerPart> {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IFeedManager _feedManager;
        private readonly IContainerService _containerService;

        public ExtendedContainerDriver(
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

        [Shape]
        public void EmptySpace(dynamic Display, dynamic Shape, TextWriter Output)
        {
            Output.WriteLine("<div style='clear:both;heigh:0px; content=\" \"'></div>");
        }
        
        protected override DriverResult Display(ContainerPart part, string displayType, dynamic shapeHelper) {

            if (part.As<AttachToProjectPart>() == null)
            {
                return null;
            }

        return ContentShape("Parts_Container_Contained", () => {
                var container = part.ContentItem;
                var query = _contentManager
                .Query(VersionOptions.Published)
                .Join<CommonPartRecord>().Where(x => x.Container.Id == container.Id)
                .Join<ContainablePartRecord>().OrderByDescending(x => x.Position);

                var metadata = container.ContentManager.GetItemMetadata(container);
                if (metadata!=null)
                _feedManager.Register(metadata.DisplayText, "rss", new RouteValueDictionary { { "containerid", container.Id } });

                var pager = new Pager(_siteService.GetSiteSettings(), part.PagerParameters);
                pager.PageSize = part.PagerParameters.PageSize != null && part.Paginated
                                ? pager.PageSize
                                : part.PageSize;

                var pagerShape = shapeHelper.Pager(pager).TotalItemCount(query.Count());
                var startIndex = part.Paginated ? pager.GetStartIndex() : 0;
                var pageOfItems = query.Slice(startIndex, pager.PageSize).ToList();

                var itemsShape = pageOfItems.Select(item => _contentManager.BuildDisplay(item, "Detail")).ToList();
                var listShape = shapeHelper.ListShape(Model: itemsShape);

                return shapeHelper.Parts_Container_Contained(
                    List: listShape,
                    Pager: part.Paginated ? pagerShape : null
                );
            });
        }
    }
}