using Orchard.ContentManagement;
using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class SuiteCRMTaskDetailViewModel
    {
        public string SuiteCRMTaskId { get; set; }
        public ContentItem OrchardCollaborationTicket { get; set; }
        public bool IsProjectTask { get; set; }
        public bool IsSync { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public string SuiteCRMUrl { get; set; }
    }
}