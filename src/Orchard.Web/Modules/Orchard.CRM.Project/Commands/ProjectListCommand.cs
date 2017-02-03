using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Commands
{
    public class ProjectListCommand : DefaultOrchardCommandHandler
    {
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;

        public ProjectListCommand(INavigationManager navigationManager, IMenuService menuService, IContentManager contentManager, ISiteService siteService, IMembershipService membershipService)
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

        [CommandName("projectlist-create")]
        [CommandHelp("projectlist-create [/MenuName:<name>] [/MenuText:<menu text>] " + "Creates projectlist menu-item")]
        [OrchardSwitches("MenuText,MenuName")]
        public void Create()
        {
            var projectList = _contentManager.Create(ContentTypes.ProjectListContentType, VersionOptions.Draft);
            var menuPart = projectList.As<MenuPart>();
            
            if (!String.IsNullOrWhiteSpace(MenuText))
            {
                var menu = _menuService.GetMenu(MenuName);

                if (menu != null)
                {
                    var menuItem = _contentManager.Create<ContentMenuItemPart>("ContentMenuItem");
                    menuItem.Content = projectList;
                    menuItem.As<MenuPart>().MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
                    menuItem.As<MenuPart>().MenuText = MenuText;
                    menuItem.As<MenuPart>().Menu = menu;
                }
            }

            _contentManager.Publish(projectList);
        }
    }
}