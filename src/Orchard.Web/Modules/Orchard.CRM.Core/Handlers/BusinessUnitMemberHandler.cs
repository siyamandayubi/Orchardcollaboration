using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.Data;

namespace Orchard.CRM.Core.Handlers
{
    public class BusinessUnitMemberHandler : ContentHandler
    {
        public BusinessUnitMemberHandler(IRepository<BusinessUnitMemberPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
   }
}