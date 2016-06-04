/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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