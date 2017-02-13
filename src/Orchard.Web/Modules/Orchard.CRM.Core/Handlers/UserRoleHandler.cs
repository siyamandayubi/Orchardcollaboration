using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;
using Orchard.Roles.Models;

namespace Orchard.CRM.Core.Handlers
{
    public class UserRoleHandler : ContentHandler
    {
        public UserRoleHandler(IRepository<UserRolesPartRecord> repository,IBasicDataService basicDataService)
        {
            Filters.Add(new ActivatingFilter<UserRolesPart>("User"));

            this.OnCreated<UserRolesPart>((contextPart, part) =>
            {
                basicDataService.ClearCache();
            });

            this.OnUpdated<UserRolesPart>((contextPart, part) =>
            {
                basicDataService.ClearCache();
            });

            this.OnRemoved<UserRolesPart>((contextPart, part) =>
            {
                basicDataService.ClearCache();
            });
        }
   }
}