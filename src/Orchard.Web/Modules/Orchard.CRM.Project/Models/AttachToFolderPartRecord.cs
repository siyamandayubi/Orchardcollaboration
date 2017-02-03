using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Models
{
    public class AttachToFolderPartRecord : ContentPartRecord
    {
        [Aggregate]
        public virtual FolderPartRecord Folder { get; set; }
    }
}