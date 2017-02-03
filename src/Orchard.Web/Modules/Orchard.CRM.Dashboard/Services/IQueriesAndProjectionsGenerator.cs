using Orchard.ContentManagement;
using Orchard.Projections.Models;

namespace Orchard.CRM.Dashboard.Services
{
    public interface IQueriesAndProjectionsGenerator : IDependency
    {
        FilterRecord CreateContentTypeFilter(string contnetType, FilterGroupRecord filterGroup);
        FilterRecord CreateFilter(string category, string type, string state, FilterGroupRecord filterGroup);
        ContentItem CreateProjection(string contentType, string queryName, string title, string itemContentType);
        ContentItem CreateProjection(string contentType, string queryName, string title, string itemContentType, int maxItems);
        QueryPart CreateQuery(string title, string contentType, string shapeName, string layoutName, bool createDefaultSortCriteria, bool filterToItemsVisibleByCurrentUser);
        QueryPart CreateQuery(string title, string contentType, string shapeName, string layoutName, string itemDisplayType, bool createDefaultSortCriteria, bool filterToItemsVisibleByCurrentUser);
        QueryPart CreateQuery(string title, string contentType);
        SortCriterionRecord CreateSortRecord(string category, string type, string state, QueryPartRecord queryPartRecord);
        QueryPart GetQuery(string title);
        LayoutRecord AddLayout(string layoutState, string itemDisplayType, QueryPart queryPart);
    }
}