using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class ProjectPartRecord : ContentPartRecord
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public ContentItemRecord Detail { get; set; }
    }
}