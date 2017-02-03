using OC.SVNConnector.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace OC.SVNConnector.Handlers
{
    public class SVNLogHandler : ContentHandler
    {
        public SVNLogHandler(IRepository<SVNLogPart> repository)
        {
            Filters.Add(new ActivatingFilter<SVNLogPart>(SVNLogPart.ContentItemTypeName));
        }
    }
}