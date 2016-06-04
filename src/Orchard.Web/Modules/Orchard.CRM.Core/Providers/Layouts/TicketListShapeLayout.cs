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