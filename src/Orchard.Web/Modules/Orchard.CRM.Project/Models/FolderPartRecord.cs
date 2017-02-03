using Orchard.ContentManagement.Records;
using Orchard.CRM.Core.Models;
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Models
{
    public class FolderPartRecord : ContentPartRecord
    {
        public virtual int? Parent_Id { get; set; }

        [Aggregate]
        public virtual ProjectPartRecord Project { get; set; }
        public string Title { get; set; }
    }
}