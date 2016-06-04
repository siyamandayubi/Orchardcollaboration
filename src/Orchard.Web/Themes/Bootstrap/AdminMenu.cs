using Orchard.Localization;
using Orchard.Themes.Services;
using Orchard.UI.Navigation;

namespace Bootstrap {
    public class AdminMenu : INavigationProvider {
        private readonly ISiteThemeService _siteThemeService;

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public AdminMenu(ISiteThemeService siteThemeService) {
            _siteThemeService = siteThemeService;
        }

        public void GetNavigation(NavigationBuilder builder) {
            var themeName = _siteThemeService.GetSiteTheme();
            if (themeName.Name == Constants.ThemeName) {
                builder.AddImageSet("themes")
                    .Add(T("Themes"), "10", BuildMenu);
            }
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            menu.Add(T(""), "10.1",
                item => item
                    .Action("Index", "Admin", new { area = Constants.RoutesAreaName })
                    .Permission(Bootstrap.Permissions.ManageThemeSettings)
            );
            menu.Add(T("Choose Options"), "10.2",
                item => item
                    .Action("Index", "Admin", new { area = Constants.RoutesAreaName })
                    .Permission(Bootstrap.Permissions.ManageThemeSettings)
            );
        }
    }
}