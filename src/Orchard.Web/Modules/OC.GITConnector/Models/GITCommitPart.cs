using Orchard.ContentManagement;
using System;

namespace OC.GITConnector.Models
{
    public class GITCommitPart : ContentPart
    {
        public const string ContentItemTypeName = "GITCommit";
        public string LogMessage
        {
            get { return this.Retrieve(x => x.LogMessage); }
            set { this.Store(x => x.LogMessage, value); }
        }

        public string Sha
        {
            get { return this.Retrieve(x => x.Sha); }
            set { this.Store(x => x.Sha, value); }
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