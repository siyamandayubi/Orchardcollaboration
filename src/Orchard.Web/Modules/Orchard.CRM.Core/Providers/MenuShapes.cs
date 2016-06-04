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
using Orchard.Core.Navigation.Services;
using Orchard.CRM.Core.Commands;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.CRM.Core.Providers
{
    public class MenuShapes : IDependency
    {
        private readonly IMenuService menuService;
        private readonly IContentManager contentManager;
        private readonly INavigationManager navigationManager;
        private readonly ITagBuilderFactory tagBuilderFactory;
        private readonly RequestContext requestContext;
        private readonly IWorkContextAccessor workContextAccessor;

        public MenuShapes(
            IWorkContextAccessor workContextAccessor,
            RequestContext requestContext,
            IMenuService menuService,
            IContentManager contentManager,
            INavigationManager navigationManager,
            ITagBuilderFactory tagBuilderFactory)
        {
            this.workContextAccessor = workContextAccessor;
            this.requestContext = requestContext;
            this.tagBuilderFactory = tagBuilderFactory;
            this.navigationManager = navigationManager;
            this.contentManager = contentManager;
            this.menuService = menuService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public static bool SortDirection(
           DynamicProjectionSortModel sortModel,
           string category,
           string type)
        {
            if (sortModel == null)
            {
                return true;
            }

            if (sortModel.Category == category && sortModel.Type == type)
            {
                return !sortModel.Direction;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// If theme is enabled, renders the stylesheets shape
        /// </summary>
        [Shape]
        public void ThemeAwareStylesheet(dynamic Shape, dynamic Display)
        {
            var context = workContextAccessor.GetContext();
            var ticketSettings = context.CurrentSite.As<TicketSettingPart>();
            if (!ticketSettings.WithoutTheme)
            {
                Display.Stylesheet();
            }
        }

        [Shape]
        public void UnauthorizedAccessShape(dynamic Shape, dynamic Display, TextWriter Output)
        {
            var title = new TagBuilder("h1");
            title.SetInnerText("Authorization error");
            Output.Write(title.ToString());

            var div = new TagBuilder("div");
            div.SetInnerText("You don't have permission to do that operation");
            Output.Write(div.ToString());
        }

        [Shape]
        public void SortLink(dynamic Shape, dynamic Display, TextWriter Output, string classNames, string text, string defaultCategory, string defaultType)
        {
            var sortModel = Services.ProjectionManagerWithDynamicSort.GetSortModelFromRequest(this.requestContext);
            string category = sortModel != null ? sortModel.Category : defaultCategory;
            string type = sortModel != null ? sortModel.Type : defaultType;

            var a = new TagBuilder("a");

            a.Attributes.Add("href", "#");
            a.Attributes.Add("data-direction", SortDirection(sortModel, defaultCategory, defaultType).ToString(CultureInfo.InvariantCulture));
            a.Attributes.Add("data-category", category);
            a.Attributes.Add("data-type", type);
            a.SetInnerText(this.T(text).ToString());

            if (!string.IsNullOrEmpty(classNames))
            {
                a.Attributes.Add("class", classNames);
            }

            Output.Write(a.ToString());
        }
    }
}