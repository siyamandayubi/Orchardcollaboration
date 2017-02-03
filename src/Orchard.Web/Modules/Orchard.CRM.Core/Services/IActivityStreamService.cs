namespace Orchard.CRM.Core.Services
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using System.Collections.Generic;
    using System.Web.Routing;

    public interface IActivityStreamService : IDependency
    {
        dynamic CreateModel(ActivityStreamRecord item);
        dynamic CreateModel(List<ActivityStreamRecord> items, int count, int page, int pageSize);
        void WriteChangesToStreamActivity(int? userId, int contentId, int versionId, ActivityStreamChangeItem[] changes, string contentDescription, RouteValueDictionary route);
        void WriteChangesToStreamActivity(int? userId, int contentId, int versionId, ActivityStreamChangeItem[] changes, string contentDescription, RouteValueDictionary route, bool createLinkToTheChange);        
        IEnumerable<ActivityStreamRecord> ActivityStreamVisibleByCurrentUser(int pageId, int pageSize);
        int ActivityStreamVisibleByCurrentUserCount();
        int ActivityStreamRestrictedByGivenQueryCount(IHqlQuery[] contentQueries);
        IEnumerable<ActivityStreamRecord> ActivityStreamRestrictedByGivenQuery(IHqlQuery[] contentQueries, int pageId, int pageSize);
        void WriteChangesToStreamActivity(ContentItem contentItem, dynamic snapshot, string mainStreamWriter);
        void WriteChangesToStreamActivity(ContentItem contentItem, dynamic snapshot, bool createdBySystem, string mainStreamWriter);
        dynamic TakeSnapshot(ContentItem contentItem);
        IEnumerable<int> LatestUsersInActivityStreamFilteredByGivenQuery(IHqlQuery contentQuery, int count);
        int ActivityStreamOfGivenUserVisibleByCurrentUserCount(int userId);
        IEnumerable<ActivityStreamRecord> ActivityStreamOfGivenUserVisibleByCurrentUser(int userId, int pageId, int pageSize);
    }
}