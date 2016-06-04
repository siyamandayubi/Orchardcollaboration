using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.CRM.Core.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Commands
{
    public class TicketsMenuCommand : DefaultOrchardCommandHandler
    {
         private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;

        public TicketsMenuCommand(INavigationManager navigationManager, IMenuService menuService, IContentManager contentManager, ISiteService siteService, IMembershipService membershipService)
        {
            _navigationManager = navigationManager;
            _menuService = menuService;
            _contentManager = contentManager;
            _siteService = siteService;
            _membershipService = membershipService;
        }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [OrchardSwitch]
        public string MenuName { get; set; }

        [CommandName("tickets-menu create")]
        [CommandHelp("tickets-menu create [/MenuName:<name>] [/MenuText:<menu text>] " + "Creates Tickets menu-item")]
        [OrchardSwitches("MenuText,MenuName")]
        public void Create()
        {
            if (!String.IsNullOrWhiteSpace(MenuText))
            {
                var menu = _menuService.GetMenu(MenuName);

                if (menu != null)
                {
                    var menuItem = _contentManager.Create<TicketMenuItemPart>("TicketMenuItem");
                    menuItem.As<MenuPart>().MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
                    menuItem.As<MenuPart>().MenuText = MenuText;
                    menuItem.As<MenuPart>().Menu = menu;
                }
            }
        }
   }
}