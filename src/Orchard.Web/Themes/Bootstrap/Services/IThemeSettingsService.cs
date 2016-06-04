using Bootstrap.Models;
using Orchard;

namespace Bootstrap.Services {
    public interface IThemeSettingsService : IDependency {
        ThemeSettingsRecord GetSettings();
    }
}
