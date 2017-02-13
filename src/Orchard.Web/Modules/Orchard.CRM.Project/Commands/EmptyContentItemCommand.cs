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