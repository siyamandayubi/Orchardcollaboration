using System;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Themes.Services;
using Orchard.UI.Admin;
using Orchard.UI.Resources;
using Bootstrap.Services;

namespace Bootstrap.Filters {
    public class LayoutFilter : FilterProvider, IResultFilter {
        private readonly IThemeSettingsService _settingsService;
        private readonly IResourceManager _resourceManager;
        private readonly ISiteThemeService _siteThemeService;

        public LayoutFilter(IThemeSettingsService settingsService, IResourceManager resourceManager, ISiteThemeService siteThemeService) {
            _settingsService = settingsService;
            _resourceManager = resourceManager;
            _siteThemeService = siteThemeService;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {

            // ignore filter on admin pages
            if (AdminFilter.IsApplied(filterContext.RequestContext))
                return;

            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
                return;

            var settings = _settingsService.GetSettings();

            if (String.IsNullOrEmpty(settings.Swatch))
                return;

            var themeName = _siteThemeService.GetSiteTheme();
            if (themeName.Name == Constants.ThemeName) {
                var viewResult = filterContext.Result as ViewResult;
                if (viewResult == null)
                    return;

                if (settings.UseFixedNav) {
                    /* TODO: Replace note use Items collection */
                    System.Web.HttpContext.Current.Items[Constants.UseFixedNav] = settings.UseFixedNav.ToString();
                }
                if (settings.UseNavSearch) {
                    /* TODO: Replace note use Items collection */
                    System.Web.HttpContext.Current.Items[Constants.UseNavSearch] = settings.UseNavSearch.ToString();
                }
                if (settings.UseFluidLayout) {
                    /* TODO: Replace note use Items collection */
                    System.Web.HttpContext.Current.Items[Constants.UseFluidLayout] = settings.UseFluidLayout.ToString();
                }
                if (settings.UseInverseNav) {
                    /* TODO: Replace note use Items collection */
                    System.Web.HttpContext.Current.Items[Constants.UseInverseNav] = settings.UseInverseNav.ToString();
                }
                if (settings.UseStickyFooter) {
                    /* TODO: Replace note use Items collection */
                    System.Web.HttpContext.Current.Items[Constants.UseStickyFooter] = settings.UseStickyFooter.ToString();
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}
