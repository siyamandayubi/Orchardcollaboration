using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentPicker.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Dashboard.Models;
using Orchard.CRM.Dashboard.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Utility;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Dashboard.Commands
{
    public class DashboardCommand : DefaultOrchardCommandHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly ISiteService _siteService;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWidgetsService _widgetsService;
        private readonly IQueriesAndProjectionsGenerator _queriesAndProjectionsGenerator;

        public DashboardCommand(
            IQueriesAndProjectionsGenerator queriesAndProjectionsGenerator,
            IWidgetsService widgetsService,
            IContentManager contentManager,
            IMembershipService membershipService,
            IAuthenticationService authenticationService,
            ISiteService siteService,
            IMenuService menuService,
            INavigationManager navigationManager)
        {
            _queriesAndProjectionsGenerator = queriesAndProjectionsGenerator;
            _widgetsService = widgetsService;
            _contentManager = contentManager;
            _membershipService = membershipService;
            _siteService = siteService;
            _menuService = menuService;
            _navigationManager = navigationManager;
            _authenticationService = authenticationService;
        }

        [OrchardSwitch]
        public string Portlets { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public bool Homepage { get; set; }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [OrchardSwitch]
        public string MenuName { get; set; }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string Name { get; set; }

        [OrchardSwitch]
        public bool RenderTitle { get; set; }

        [OrchardSwitch]
        public string Zone { get; set; }

        [OrchardSwitch]
        public string WidgetPosition { get; set; }

        [OrchardSwitch]
        public string Layer { get; set; }

        [OrchardSwitch]
        public string Identity { get; set; }

        [OrchardSwitch]
        public string Text { get; set; }

        [OrchardSwitch]
        public bool NotCollapsible { get; set; }

        [OrchardSwitch]
        public bool ShowConfigurableItemsCountInHeader { get; set; }

        [OrchardSwitch]
        public bool Publish { get; set; }

        [CommandName("dashboard create")]
        [CommandHelp("dashboard create [/Portlets:<portlets>] /Title:<title> /Path:<path> [/Text:<text>] [/Owner:<username>] [/MenuName:<name>] [/MenuText:<menu text>] [/Homepage:true|false] [/ShowConfigurableItemsCountInHeader:<ShowConfigurableItemsCountInHeader>]")]
        [OrchardSwitches("Portlets,Title,Owner,MenuText,Homepage,MenuName,ShowConfigurableItemsCountInHeader")]
        public void Create()
        {
            if (String.IsNullOrEmpty(Owner))
            {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }

            var owner = _membershipService.GetUser(Owner);
            _authenticationService.SetAuthenticatedUserForRequest(owner);

            var dashboard = _contentManager.Create(Consts.GenericDashboardContentType, VersionOptions.Draft);
            dashboard.As<CommonPart>().Owner = owner;
            dashboard.As<TitlePart>().Title = Title;

            if (!String.IsNullOrWhiteSpace(MenuText))
            {
                var menu = _menuService.GetMenu(MenuName);

                if (menu != null)
                {
                    var menuItem = _contentManager.Create<ContentMenuItemPart>("ContentMenuItem");
                    menuItem.Content = dashboard;
                    menuItem.As<MenuPart>().MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
                    menuItem.As<MenuPart>().MenuText = MenuText;
                    menuItem.As<MenuPart>().Menu = menu;
                }
            }

            if (Homepage)
            {
                AutoroutePart autoroutePart = dashboard.As<AutoroutePart>();
                if (autoroutePart != null)
                {
                    autoroutePart.UseCustomPattern = true;
                    autoroutePart.CustomPattern = "/";
                }
            }

            var dashboardPart = dashboard.As<GenericDashboardPart>();
            dashboardPart.PortletList = Portlets;
            dashboardPart.ShowConfigurableItemsCountInHeader = ShowConfigurableItemsCountInHeader;
            dashboardPart.CreatePortletsOnPublishing = true;
            _contentManager.Publish(dashboard);
        }

        [CommandName("widget-generic-dashboard create")]
        [CommandHelp("widget-generic-dashboard create /Portlets:<portlets> /Title:<title> /Name:<name> /Zone:<zone> /WidgetPosition:<widgetposition> /Layer:<layer> [/Identity:<identity>] [/RenderTitle:true|false] [/Owner:<owner>] [/Text:<text>]  [/ShowConfigurableItemsCountInHeader:<ShowConfigurableItemsCountInHeader>] [/MenuName:<name>]\r\n\t" + "Creates a new widget")]
        [OrchardSwitches("NotCollapsible,Portlets,Title,Name,Zone,WidgetPosition,Layer,Identity,Owner,Text,MenuName,RenderTitle,ShowConfigurableItemsCountInHeader")]
        public void CreateDashboardWidget()
        {
            string type = Consts.GenericCoverWidgetContentType;
            var widget = CreateWidget(type);
            if (widget == null)
            {
                return;
            }

            if (String.IsNullOrEmpty(Owner))
            {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }

            var owner = _membershipService.GetUser(Owner);
            _authenticationService.SetAuthenticatedUserForRequest(owner);

            var dashboard = _contentManager.Create(Consts.GenericDashboardContentType, VersionOptions.Draft);
            dashboard.As<ICommonPart>().Owner = owner;
            dashboard.As<TitlePart>().Title = Title;
            var dashboardPart = dashboard.As<GenericDashboardPart>();
            dashboardPart.PortletList = Portlets;
            dashboardPart.ShowConfigurableItemsCountInHeader = ShowConfigurableItemsCountInHeader;
            dashboardPart.CreatePortletsOnPublishing = true;
            dashboardPart.ShowCollapsedInInitializedState = true;
            dashboardPart.Collapsiable = !NotCollapsible;
            _contentManager.Publish(dashboard);

            var coverWidgetPart = widget.As<CoverWidgetPart>();
            coverWidgetPart.HideEditLinkInFrontendLoadSync = true;
            coverWidgetPart.TargetContentItemId = dashboard.Id;
            coverWidgetPart.LoadSync = true;

            _contentManager.Publish(widget.ContentItem);
            Context.Output.WriteLine(T("Widget created successfully.").Text);
        }

        [CommandName("widget-sidebar create")]
        [CommandHelp("widget-sidebar create /Portlets:<portlets> /Name:<name> /Zone:<zone> /WidgetPosition:<widgetposition> /Layer:<layer> [/Identity:<identity>] [/RenderTitle:true|false] [/Owner:<owner>] [/Text:<text>] [/MenuName:<name>] [/Title:<title>]\r\n\t" + "Creates a new widget")]
        [OrchardSwitches("Name,Zone,WidgetPosition,Layer,Identity,Owner,Text,MenuName,RenderTitle,Portlets,Title")]
        public void CreateSidebarWidget()
        {
            if (String.IsNullOrEmpty(Owner))
            {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }

            var owner = _membershipService.GetUser(Owner);
            _authenticationService.SetAuthenticatedUserForRequest(owner);


            string type = Consts.SidebarWidgetType;
            var widget = CreateWidget(type);
            if (widget == null)
            {
                return;
            }

            var sidebarPart = widget.As<CoverWidgetPart>();
            var dashboard = _contentManager.Create(Consts.SidebarDashboardType, VersionOptions.Draft);
            dashboard.As<ICommonPart>().Owner = owner;
            SidebarDashboardPart sidebarDashboardPart = dashboard.As<SidebarDashboardPart>();
            sidebarDashboardPart.SidebarPortletList = Portlets;
            dashboard.As<TitlePart>().Title = Title;

            _contentManager.Publish(dashboard);

            sidebarPart.TargetContentItemId = dashboard.Id;

            _contentManager.Publish(widget.ContentItem);
            Context.Output.WriteLine(T("Widget created successfully.").Text);
        }

        private LayerPart GetLayer(string layer)
        {
            var layers = _widgetsService.GetLayers();
            return layers.FirstOrDefault(layerPart => String.Equals(layerPart.Name, layer, StringComparison.OrdinalIgnoreCase));
        }

        private WidgetPart CreateWidget(string type)
        {
            var widgetTypeNames = _widgetsService.GetWidgetTypeNames().ToList();

            var layer = GetLayer(Layer);
            if (layer == null)
            {
                Context.Output.WriteLine(T("Creating widget failed : layer {0} was not found.", Layer));
                return null;
            }

            var widget = _widgetsService.CreateWidget(layer.ContentItem.Id, type, T(Title).Text, WidgetPosition, Zone);

            if (!String.IsNullOrWhiteSpace(Name))
            {
                widget.Name = Name.Trim();
            }

            widget.RenderTitle = RenderTitle;

            if (widget.Has<MenuWidgetPart>() && !String.IsNullOrWhiteSpace(MenuName))
            {
                var menu = _menuService.GetMenu(MenuName);

                if (menu != null)
                {
                    widget.RenderTitle = false;
                    widget.As<MenuWidgetPart>().MenuContentItemId = menu.ContentItem.Id;
                }
            }

            if (String.IsNullOrEmpty(Owner))
            {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }
            var owner = _membershipService.GetUser(Owner);
            widget.As<ICommonPart>().Owner = owner;

            if (widget.Has<IdentityPart>() && !String.IsNullOrEmpty(Identity))
            {
                widget.As<IdentityPart>().Identifier = Identity;
            }

            return widget;
        }
    }
}