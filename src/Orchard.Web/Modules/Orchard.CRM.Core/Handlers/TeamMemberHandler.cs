using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.Data;

namespace Orchard.CRM.Core.Handlers
{
    public class TeamMemberHandler : ContentHandler
    {
        public TeamMemberHandler(IRepository<TeamMemberPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));

            this.OnRemoved<TeamMemberPart>((contextPart, part) =>
            {
                repository.Delete(part.Record);
            });
        }
   }
}