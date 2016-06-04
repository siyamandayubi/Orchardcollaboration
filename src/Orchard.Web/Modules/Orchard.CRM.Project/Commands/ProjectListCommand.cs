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