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

using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Providers.Layouts;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Providers
{
    public class ProjectDashboardListLayout : ILayoutProvider
    {
        private readonly IContentManager contentManager;
        private readonly ShapeLayout shapeLayout;
        protected dynamic Shape { get; set; }

        public ProjectDashboardListLayout(IShapeFactory shapeFactory, IContentManager contentManager)
        {
            this.contentManager = contentManager;
            Shape = shapeFactory;
            this.shapeLayout = new ShapeLayout(shapeFactory, contentManager);
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeLayoutContext describe)
        {
            describe.For("Html", T("Html"), T("Html Layouts"))
                .Element(QueryNames.ProjectDashboardListLayout, T("Project Dashboard List of items portlet"), T("Uses a build-in shape to render list of items as a Project portlet."),
                    DisplayLayout,
                    RenderLayout,
                    "ShapeLayout"
                );
        }

        public LocalizedString DisplayLayout(LayoutContext context)
        {
            return T("Renders list of items as a Project Dashboard portlet");
        }

        public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults)
        {
            context.State = new JObject();
            context.State.ShapeType = QueryNames.ProjectSummaryProjectionPortletShapeName;
            context.State.DisplayType = "Summary";

            return this.shapeLayout.RenderLayout(context, layoutComponentResults);
        }
    }
}