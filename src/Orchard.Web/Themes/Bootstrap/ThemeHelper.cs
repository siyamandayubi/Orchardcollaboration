using Orchard;

namespace Bootstrap {
    public static class ThemeHelper {

        /// <summary>
        /// Retrieves settings for the theme from Orchard's WorkContext.
        /// <param name="workContext">The WorkContext to retrieve the settings from.</param>
        /// <param name="itemName">The setting name to retrieve.</param>
        /// <returns>A boolean representing the current Theme Setting.</returns>
        public static bool SettingsEval(this WorkContext workContext, string itemName) {
            var returnValue = false;
            var context = workContext.HttpContext;
            if (context.Items[itemName] != null && (string)context.Items[itemName] == bool.TrueString) {
                returnValue = true;
            }
            return returnValue;
        }

        /// <summary>
        /// Returns the correct Bootstrap CSS class for the current Aside Zone configuration.
        /// </summary>
        /// <param name="asideClass">The current Aside Zone configuration.</param>
        /// <returns>A string representing the correct Boostrap CSS class for the zone configuration.</returns>
        public static string GetAsideCssClass(string asideClass) {
            switch (asideClass) {
                case "aside-1":
                case "aside-2":
                    return "col-lg-9";
                case "aside-12":
                    return "col-lg-6";
                default:
                    return "col-lg-12";
            }
        }

        /// <summary>
        /// Returns the correct Bootstrap CSS class for the current Tripel Zone configuration.
        /// </summary>
        /// <param name="tripelClass">The current Tripel Zone configuration.</param>
        /// <returns>A string representing the correct Boostrap CSS class for the zone configuration.</returns>
        public static string GetTripelCssClass(string tripelClass) {
            switch (tripelClass) {
                case "tripel-12":
                case "tripel-23":
                case "tripel-13":
                    return "col-lg-6";
                case "tripel-123":
                    return "col-lg-4";
                default:
                    return "col-lg-12";
            }
        }

        /// <summary>
        /// Returns the correct Bootstrap CSS class for the current FooterQuad Zone configuration.
        /// </summary>
        /// <param name="footerQuadClass">The current FooterQuad Zone configuration.</param>
        /// <returns>A string representing the correct Boostrap CSS class for the zone configuration.</returns>
        public static string GetFooterQuadCssClass(string footerQuadClass) {
            switch (footerQuadClass) {
                case "split-12":
                case "split-13":
                case "split-14":
                case "split-23":
                case "split-24":
                case "split-34":
                    return "col-lg-6";
                case "split-123":
                case "split-124":
                case "split-134":
                case "split-234":
                    return "col-lg-4";
                case "split-1234":
                    return "col-lg-3";
                default:
                    return "col-lg-12";
            }
        }

    }
}