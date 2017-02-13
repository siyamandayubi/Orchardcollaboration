namespace Orchard.CRM.Core.Providers.ActivityStream
{
    using Orchard.ContentManagement;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IActivityStreamWriter : IDependency
    {
        bool CanApply(ActiviyStreamWriterContext context);
        IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context);

        /// <summary>
        /// The return value contains description and the weight of the description.
        /// The system selects a description with highest weight
        /// </summary>
        ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context);
        string Name { get; }
        dynamic TakeSnapshot(ContentItem contentItem);
    }
}
