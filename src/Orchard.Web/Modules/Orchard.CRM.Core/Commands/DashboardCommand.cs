using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentPicker.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Utility;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Commands
{
    public class DashboardCommand : DefaultOrchardCommandHandler
    {
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWidgetsService widgetsService;

        public DashboardCommand(
            IWidgetsService widgetsService,
            INavigationManager navigationManager,
            IMenuService menuService,
            IAuthenticationService authenticationService,
            IContentManager contentManager,
            ISiteService siteService,
            IMembershipService membershipService)
        {
            this.widgetsService = widgetsService;
            _navigationManager = navigationManager;
            _menuService = menuService;
            _contentManager = contentManager;
            _siteService = siteService;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
        }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public bool Homepage { get; set; }

        [OrchardSwitch]
        public string Slug { get; set; }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [OrchardSwitch]
        public string MenuName { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public string LayerName { get; set; }

        [OrchardSwitch]
        public string Zone { get; set; }

        [OrchardSwitch]
        public string WidgetPosition { get; set; }

        [OrchardSwitch]
        public bool RenderTitle { get; set; }

        [CommandName("ticket-dashboard create")]
        [CommandHelp("ticket-dashboard create [/Slug:<slug>] /Title:<title> [/Owner:<username>] [/MenuName:<name>] [/MenuText:<menu text>] [/Homepage:true|false] " + "Creates Tickets dashboard")]
        [OrchardSwitches("Slug,Title,Owner,MenuText,MenuName,Homepage")]
        public void Create()
        {
            if (String.IsNullOrEmpty(Owner))
            {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }

            var owner = _membershipService.GetUser(Owner);
            _authenticationService.SetAuthenticatedUserForRequest(owner);

            var dashboard = _contentManager.Create("Dashboard", VersionOptions.Draft);
            dashboard.As<TitlePart>().Title = Title;
            dashboard.As<ICommonPart>().Owner = owner;

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

            // (PH:Autoroute) Hackish way to leave Slug and Homepage switches intact without requiring a dependency on Autoroute. This may throw an Exception with
            // no AutoroutePart. But it means that normal setup recipes will still be able to give you a homepage without issue.
            if (Homepage || !String.IsNullOrWhiteSpace(Slug))
            {
                dynamic dpage = dashboard;
                if (dpage.AutoroutePart != null)
                {
                    dpage.AutoroutePart.UseCustomPattern = true;
                    dpage.AutoroutePart.CustomPattern = Homepage ? "/" : Slug;
                }
            }

            _contentManager.Publish(dashboard);
            Context.Output.WriteLine(T("Dashboard created successfully.").Text);

        }

        [CommandName("ticket-dashboard-widget create")]
        [CommandHelp("ticket-dashboard-widget create [/Slug:<slug>] /Title:<title> [/Owner:<username>] [/LayerName:<name>] [/Zone:<widget zone>] [/WidgetPosition:<widget position>] [/RenderTitle:true|false] " + "Creates Tickets dashboard")]
        [OrchardSwitches("Slug,Title,Owner,LayerName,Zone,WidgetPosition,RenderTitle")]
        public void CreateWidget()
        {
            if (String.IsNullOrEmpty(Owner))
            {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }

            var owner = _membershipService.GetUser(Owner);
            _authenticationService.SetAuthenticatedUserForRequest(owner);

            var layers = this.widgetsService.GetLayers();
            var layer = layers.First(c => c.Name == LayerName);
            var dashboard = widgetsService.CreateWidget(layer.Id, "TicketsDashboardWidget", Title, WidgetPosition, Zone);
            dashboard.Record.RenderTitle = RenderTitle;

            dashboard.As<ICommonPart>().Owner = owner;

            _contentManager.Publish(dashboard.ContentItem);
            Context.Output.WriteLine(T("Dashboard created successfully.").Text);

        }
    }
}