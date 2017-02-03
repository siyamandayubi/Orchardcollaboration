using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Models
{
    public class SuiteCRMTaskPart : ContentPart
    {
        public const string SuiteCRMProjectTaskTypeValue = "ProjectTask";
        public const string SuiteCRMTaskTypeValue = "Tasks";

        public string ExternalId
        {
            get
            {
                return this.Retrieve(c => c.ExternalId);
            }
            set
            {
                this.Store(x => x.ExternalId, value);
            }
        }

        public string TaskType
        {
            get
            {
                return this.Retrieve(c => c.TaskType);
            }
            set
            {
                this.Store(x => x.TaskType, value);
            }

        }

        public DateTime LastSyncTime
        {
            get
            {
                return this.Retrieve(c => c.LastSyncTime);
            }
            set
            {
                this.Store(c => c.LastSyncTime, value);
            }
        }
    }
}