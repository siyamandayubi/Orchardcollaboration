using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;

namespace Orchard.CRM.Core.Handlers
{
    public class BusinessUnitHandler : ContentHandler
    {
        public BusinessUnitHandler(IRepository<BusinessUnitPartRecord> repository, IBasicDataService basicDataService)
        {
            Filters.Add(StorageFilter.For(repository));

            this.OnCreated<BusinessUnitPart>((context, part) =>
            {
                basicDataService.ClearCache();
            });

            this.OnUpdated<BusinessUnitPart>((context, part) =>
            {
                basicDataService.ClearCache();
            });

            this.OnRemoved<BusinessUnitPart>((context, part) =>
            {
                basicDataService.ClearCache();
            });
        }
   }
}