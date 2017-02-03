using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;

namespace Orchard.CRM.Core.Handlers
{
    public class TeamHandler : ContentHandler
    {
        public TeamHandler(IRepository<TeamPartRecord> repository, IBasicDataService basicDataService)
        {
            Filters.Add(StorageFilter.For(repository));
            this.OnCreated<TeamPart>((context, part) =>
            {
                basicDataService.ClearCache();
            });

            this.OnUpdated<TeamPart>((context, part) =>
            {
                basicDataService.ClearCache();
            });

            this.OnRemoved<TeamPart>((context, part) =>
            {
                basicDataService.ClearCache();
            });
        }
   }
}