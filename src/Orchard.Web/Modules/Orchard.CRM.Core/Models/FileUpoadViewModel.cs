using Orchard.CRM.Core.Settings;

namespace Orchard.CRM.Core.Models
{
    public class FileUpoadViewModel
    {
        public int ContentId { get; set; }
        public string Guid { get; set; }
        public string ContentType { get; set; }
        public FileUploadPartSettings Settings { get; set; }
    }
}
