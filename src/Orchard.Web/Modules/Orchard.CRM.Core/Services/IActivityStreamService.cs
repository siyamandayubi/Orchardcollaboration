/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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