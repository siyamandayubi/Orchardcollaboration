using Orchard.ContentManagement;
using System;

namespace OC.SVNConnector.Models
{
    public class SVNLogPart : ContentPart
    {
        public const string ContentItemTypeName = "SVNLog";
        public string LogMessage
        {
            get { return this.Retrieve(x => x.LogMessage); }
            set { this.Store(x => x.LogMessage, value); }
        }

        public long Revision
        {
            get { return this.Retrieve(x => x.Revision); }
            set { this.Store(x => x.Revision, value); }
        }

        public DateTime Time
        {
            get { return this.Retrieve(x => x.Time); }
            set { this.Store(x => x.Time, value); }
        }

        public string Author
        {
            get { return this.Retrieve(x => x.Author); }
            set { this.Store(x => x.Author, value); }
        }

        public string LogOrigin
        {
            get { return this.Retrieve(x => x.LogOrigin); }
            set { this.Store(x => x.LogOrigin, value); }
        }        
    }
}