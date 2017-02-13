using OC.GITConnector.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace OC.GITConnector.Handlers
{
    public class GITCommitHandler : ContentHandler
    {
        public GITCommitHandler(IRepository<GITCommitPart> repository)
        {
            Filters.Add(new ActivatingFilter<GITCommitPart>(GITCommitPart.ContentItemTypeName));
        }
    }
}