using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.CRM.TimeTracking.Models;
using Orchard.Data;

namespace Orchard.CRM.TimeTracking.Handlers
{
    public class TimeTrackingHandler : ContentHandler
    {
        public TimeTrackingHandler(IRepository<TimeTrackingPartRecord> repository, IContentManager contentManager)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}