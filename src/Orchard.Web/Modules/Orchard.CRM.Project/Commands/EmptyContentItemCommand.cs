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

using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentPicker.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;
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
    public class EmptyContentItemCommand : DefaultOrchardCommandHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly ISiteService _siteService;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;
        private readonly IAuthenticationService _authenticationService;

        public EmptyContentItemCommand(
            IContentManager contentManager,
            IMembershipService membershipService,
            IAuthenticationService authenticationService,
            ISiteService siteService,
            IMenuService menuService,
            INavigationManager navigationManager)
        {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _siteService = siteService;
            _menuService = menuService;
            _navigationManager = navigationManager;
            _authenticationService = authenticationService;
        }

        [OrchardSwitch]
        public string Slug { get; set; }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string Path { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public bool Homepage { get; set; }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [OrchardSwitch]
        public string MenuName { get; set; }

        [CommandName("empty-item create")]
        [CommandHelp("empty-item create [/Slug:<slug>] /Title:<title> /Path:<path> [/Text:<text>] [/Owner:<username>] [/MenuName:<name>] [/MenuText:<menu text>] [/Homepage:true|false]")]
        [OrchardSwitches("Slug,Title,Path,Owner,MenuText,Homepage,MenuName")]
        public void Create()
        {
            if (String.IsNullOrEmpty(Owner))
            {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }

            var owner = _membershipService.GetUser(Owner);
            _authenticationService.SetAuthenticatedUserForRequest(owner);

            var emptyItem = _contentManager.Create(ContentTypes.EmptyContentType, VersionOptions.Draft);
            emptyItem.As<ICommonPart>().Owner = owner;
            emptyItem.As<TitlePart>().Title = Title;

            if (!String.IsNullOrWhiteSpace(MenuText))
            {
                var menu = _menuService.GetMenu(MenuName);

                if (menu != null)
                {
                    var menuItem = _contentManager.Create<ContentMenuItemPart>("ContentMenuItem");
                    menuItem.Content = emptyItem;
                    menuItem.As<MenuPart>().MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
                    menuItem.As<MenuPart>().MenuText = MenuText;
                    menuItem.As<MenuPart>().Menu = menu;
                }
            }

            if (Homepage)
            {
                AutoroutePart autoroutePart = emptyItem.As<AutoroutePart>();
                if (autoroutePart != null)
                {
                    autoroutePart.UseCustomPattern = true;
                    autoroutePart.CustomPattern = "/";
                }
            }

            _contentManager.Publish(emptyItem);

            Context.Output.WriteLine(T("EmptyItem created successfully.").Text);

        }
    }
}