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
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Settings.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using System.Web;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.CRM.Core.Services
{
    public class WidgetService : IWidgetService
    {
       private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRuleManager _ruleManager;
        private readonly IWidgetsService _widgetsService;
        private readonly IOrchardServices _orchardServices;

        public WidgetService(
            IWorkContextAccessor workContextAccessor, 
            IRuleManager ruleManager, 
            IWidgetsService widgetsService,
            IOrchardServices orchardServices) {
            _workContextAccessor = workContextAccessor;
            _ruleManager = ruleManager;
            _widgetsService = widgetsService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; private set; }

        public void GetWidgets(dynamic model, HttpContextBase context)
        {
            var workContext = _workContextAccessor.GetContext(context);

            if (workContext == null ||
                workContext.Layout == null ||
                workContext.CurrentSite == null ||
                AdminFilter.IsApplied(context.Request.RequestContext) ||
                !ThemeFilter.IsApplied(context.Request.RequestContext))
            {
                return;
            }

            // Once the Rule Engine is done:
            // Get Layers and filter by zone and rule
            IEnumerable<LayerPart> activeLayers = _orchardServices.ContentManager.Query<LayerPart, LayerPartRecord>().List();

            var activeLayerIds = new List<int>();
            foreach (var activeLayer in activeLayers)
            {
                // ignore the rule if it fails to execute
                try
                {
                    if (_ruleManager.Matches(activeLayer.Record.LayerRule))
                    {
                        activeLayerIds.Add(activeLayer.ContentItem.Id);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warning(e, T("An error occured during layer evaluation on: {0}", activeLayer.Name).Text);
                }
            }

            IEnumerable<WidgetPart> widgetParts = _widgetsService.GetWidgets(layerIds: activeLayerIds.ToArray());

            var defaultCulture = workContext.CurrentSite.As<SiteSettingsPart>().SiteCulture;
            var currentCulture = workContext.CurrentCulture;

            var zones = model.Zones;

            foreach (var widgetPart in widgetParts)
            {
                var commonPart = widgetPart.As<ICommonPart>();
                if (commonPart == null || commonPart.Container == null)
                {
                    Logger.Warning("The widget '{0}' is has no assigned layer or the layer does not exist.", widgetPart.Title);
                    continue;
                }

                // ignore widget for different cultures
                var localizablePart = widgetPart.As<ILocalizableAspect>();
                if (localizablePart != null)
                {
                    // if localized culture is null then show if current culture is the default
                    // this allows a user to show a content item for the default culture only
                    if (localizablePart.Culture == null && defaultCulture != currentCulture)
                    {
                        continue;
                    }

                    // if culture is set, show only if current culture is the same
                    if (localizablePart.Culture != null && localizablePart.Culture != currentCulture)
                    {
                        continue;
                    }
                }

                // check permissions
                if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, widgetPart))
                {
                    continue;
                }

                var widgetShape = _orchardServices.ContentManager.BuildDisplay(widgetPart);
                zones[widgetPart.Record.Zone].Add(widgetShape, widgetPart.Record.Position);
            }
       }
    }
}