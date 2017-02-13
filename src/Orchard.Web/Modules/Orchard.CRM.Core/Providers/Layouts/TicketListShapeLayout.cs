using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.CRM.Core.Providers.Layouts
{
    public class TicketListShapeLayout : ILayoutProvider
    {
        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }
        public const string ShapeType = "TicketsListShape";

        public TicketListShapeLayout(IShapeFactory shapeFactory, IContentManager contentManager)
        {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeLayoutContext describe)
        {
            describe.For("Html", T("Html"), T("Html Layouts"))
                .Element("Tickets List", T("Tickets List"), T("Uses a build-in list-based shape to render a list of tickets. (This layout doesn't support 'Properties Display Mode')"),
                    DisplayLayout,
                    RenderLayout,
                    string.Empty
                );
        }

        public LocalizedString DisplayLayout(LayoutContext context)
        {
            return T("Ticket list shape");
        }

        public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults)
        {
            if (context.LayoutRecord.Display == (int)LayoutRecord.Displays.Properties)
            {
                throw new ArgumentOutOfRangeException("LayoutRecord.Displays.Properties is not supported by this Layout");
            }

            dynamic shape = ((IShapeFactory)Shape).Create(ShapeType);
            shape.ContentItems = layoutComponentResults.Select(x => x.ContentItem);
            shape.BuildShapes = this.BuildShapes(context, layoutComponentResults);
            shape.DisplayType = context.LayoutRecord.DisplayType;

            return shape;
        }

        private IEnumerable<dynamic> BuildShapes(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults)
        {
            List<dynamic> returnValue = new List<dynamic>();

            foreach(var item in layoutComponentResults)
            {
                returnValue.Add(_contentManager.BuildDisplay(item.ContentItem, context.LayoutRecord.DisplayType));
            }

            return returnValue;
        }
    }
}