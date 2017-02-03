namespace Orchard.CRM.Core.Providers.ActivityStream
{
    using Orchard.ContentManagement;
    using Orchard.Security;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ActiviyStreamWriterContext
    {
        public ActiviyStreamWriterContext(
            ContentItem newPart,
            dynamic snapshot,
            IUser user)
        {
            this.Snapshot = snapshot;
            this.ContentItem = newPart;
            this.User = user;
        }

        public dynamic Snapshot { get; private set; }
        public ContentItem ContentItem { get; private set; }
        public IUser User { get; private set; }
    }
}