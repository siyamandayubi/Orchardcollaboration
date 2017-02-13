using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.CRM.Core.Models
{
    public class FileUploadPartRecord : ContentPartRecord
    {
        public virtual Guid FolderGuid { get; set; }
        public virtual int FilesCount { get; set; }
    }

    public class FileUploadPart : ContentPart<FileUploadPartRecord>
    {
        public Guid Guid
        {
            get { return Record.FolderGuid; }
            set { Record.FolderGuid = value; }
        }
    }
}
