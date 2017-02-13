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
    public class ProjectDashboardTicketsListLayout : ILayoutProvider
    {
        private readonly IContentManager contentManager;
        private readonly ShapeLayout shapeLayout;
        protected dynamic Shape { get; set; }

        public ProjectDashboardTicketsListLayout(IShapeFactory shapeFactory, IContentManager contentManager)
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
                .Element(QueryNames.ProjectDashboardListLayout, T("Project Dashboard Ticket list portlet"), T("Uses a build-in shape to render list of tickets inside a Project portlet."),
                    DisplayLayout,
                    RenderLayout,
                    "ShapeLayout"
                );
        }

        public LocalizedString DisplayLayout(LayoutContext context)
        {
            return T("Renders list of tickets as a Project Dashboard portlet");
        }

        public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults)
        {
            context.State = new JObject();
            context.State.ShapeType = QueryNames.ProjectTicketsSummaryShapeName;
            context.State.DisplayType = "Summary";

            return this.shapeLayout.RenderLayout(context, layoutComponentResults);
        }
    }
}