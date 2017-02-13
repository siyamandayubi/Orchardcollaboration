using Orchard.ContentManagement;

namespace Orchard.CRM.Core.Services
{
    public interface IExtendedContentManager : IDependency
    {
        ContentItem Get(int id, string contentType);
        ContentItem Get(int id, VersionOptions options, string contentType);
        ContentItem Get(int id, VersionOptions options, QueryHints hints, string contentType);
    }
}