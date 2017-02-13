using Orchard.ContentManagement;
using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class SuiteCRMProjectDetailViewModel
    {
        public SuiteCRMViewModel SuiteCRMProject { get; set; }
        public ContentItem OrchardCollaborationProject { get; set; }
        public dynamic OrchardProjectShape { get; set; }
        public string OrchardCollaborationTitle { get; set; }
        public bool IsSync { get; set; }
        public DateTime? LastSyncTime { get; set; }
    }
}