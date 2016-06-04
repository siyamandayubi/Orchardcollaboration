using System.Linq;
using Orchard.Data;
using Bootstrap.Models;

namespace Bootstrap.Services {
    public class SettingsService : IThemeSettingsService {
        private readonly IRepository<ThemeSettingsRecord> _repository;

        public SettingsService(IRepository<ThemeSettingsRecord> repository) {
            _repository = repository;
        }

        public ThemeSettingsRecord GetSettings() {
            var settings = _repository.Table.SingleOrDefault();
            if (settings == null) {
                _repository.Create(settings = new ThemeSettingsRecord());
            }

            return settings;
        }
    }
}
