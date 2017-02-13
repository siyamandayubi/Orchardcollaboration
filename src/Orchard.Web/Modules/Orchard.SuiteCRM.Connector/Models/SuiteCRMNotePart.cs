using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Models
{
    public class SuiteCRMNotePart : ContentPart
    {
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