using Orchard.CRM.Project.Settings;

namespace Orchard.CRM.Project.ViewModels {
    public class FileFieldViewModel {
        public FileFieldSettings Settings { get; set; }
        public Fields.FileField Field { get; set; }
    }
}