using System.Web.Mvc;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;
using Bootstrap.ViewModels;
using Bootstrap.Services;

namespace Bootstrap.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IThemeSettingsService _settingsService;

        public AdminController(
            IOrchardServices services,
            IThemeSettingsService settingsService) {
            _settingsService = settingsService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            var settings = _settingsService.GetSettings();

            var viewModel = new ThemeSettingsViewModel {
                Swatch = settings.Swatch,
                UseFixedNav = settings.UseFixedNav,
                UseNavSearch = settings.UseNavSearch,
                UseFluidLayout = settings.UseFluidLayout,
                UseInverseNav = settings.UseInverseNav,
                UseStickyFooter = settings.UseStickyFooter
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(ThemeSettingsViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Bootstrap.Permissions.ManageThemeSettings, T("Couldn't update Bootstrap settings")))
                return new HttpUnauthorizedResult();

            var settings = _settingsService.GetSettings();
            settings.Swatch = viewModel.Swatch;
            settings.UseFixedNav = viewModel.UseFixedNav;
            settings.UseNavSearch = viewModel.UseNavSearch;
            settings.UseFluidLayout = viewModel.UseFluidLayout;
            settings.UseInverseNav = viewModel.UseInverseNav;
            settings.UseStickyFooter = viewModel.UseStickyFooter;

            Services.Notifier.Information(T("Your settings have been saved."));

            return View(viewModel);
        }
    }
}
