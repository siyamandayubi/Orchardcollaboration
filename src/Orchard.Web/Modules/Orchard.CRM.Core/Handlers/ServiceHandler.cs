using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;

namespace Orchard.CRM.Core.Handlers
{
    public class ServiceHandler : ContentHandler
    {
        public ServiceHandler(IRepository<ServicePartRecord> repository, IBasicDataService basicDataService)
        {
            Filters.Add(StorageFilter.For(repository));

            this.OnCreated<ServicePart>((context, part) =>
            {
                basicDataService.ClearCache();
            });

            this.OnUpdated<ServicePart>((context, part) =>
            {
                basicDataService.ClearCache();
            });

            this.OnRemoved<ServicePart>((context, part) =>
            {
                basicDataService.ClearCache();
            });
        }
   }
}