namespace Orchard.CRM.Core.Services
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NHibernate.Transform;
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Records;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers.Filters;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Users.Models;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using Orchard.ContentManagement.MetaData.Models;
    using Orchard.ContentManagement.FieldStorage.InfosetStorage;
    using Orchard.Security;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using System.Web.Routing;
    using Orchard.UI.Navigation;
    using Orchard.Workflows.Services;
    using Orchard.CRM.Core.Activities;
    using Orchard.Core.Common.Models;

    public class ActivityStreamService : BaseService, IActivityStreamService
    {
        private readonly IRepository<ActivityStreamRecord> repository;
        private readonly Lazy<ISessionLocator> sessionLocator;
        private readonly IBasicDataService basicDataService;
        private readonly IEnumerable<IActivityStreamWriter> activityStreamWriters;
        private readonly IWorkflowManager workflowManager;
        private readonly ITransactionManager transactionManager;
        private readonly IMembershipService _memebershipService;

        public Localizer T { get; set; }

        public ActivityStreamService(
            ITransactionManager transactionManager,
            IWorkflowManager workflowManager,
            IBasicDataService basicDataService,
            IRepository<ActivityStreamRecord> repository,
            IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort,
            IOrchardServices services,
            IEnumerable<IActivityStreamWriter> activityStreamWriters,
            Lazy<ISessionLocator> sessionLocator,
            IMembershipService memebershipService)
            : base(services, projectionManagerWithDynamicSort)
        {
            this.transactionManager = transactionManager;
            this.workflowManager = workflowManager;
            this.activityStreamWriters = activityStreamWriters;
            this.sessionLocator = sessionLocator;
            this.repository = repository;
            this.basicDataService = basicDataService;

            _memebershipService = memebershipService;

            this.T = NullLocalizer.Instance;
        }

        public int ActivityStreamVisibleByCurrentUserCount()
        {
            if (this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
            {
                return this.repository.Table.Count();
            }

            var contentQuery = this.CreateBaseQuery();

            return this.Count(new[] { contentQuery });
        }

        public IEnumerable<ActivityStreamRecord> ActivityStreamRestrictedByGivenQuery(IHqlQuery[] contentQueries, int pageId, int pageSize)
        {
            if (contentQueries == null) { throw new ArgumentNullException("contentQueries"); }

            var queries = new List<IHqlQuery>();
            foreach (var query in contentQueries)
            {
                queries.Add(this.ApplyContentPermissionFilter(query));
            }

            return this.RunQuery(queries.ToArray(), pageId, pageSize);
        }

        public IEnumerable<int> LatestUsersInActivityStreamFilteredByGivenQuery(IHqlQuery contentQuery, int count)
        {
            if (contentQuery == null) { throw new ArgumentNullException("contentQuery"); }

            contentQuery = this.ApplyContentPermissionFilter(contentQuery);

            DefaultHqlQuery defaultQuery = contentQuery as DefaultHqlQuery;
            var hql = defaultQuery.ToHql(true);
            hql = string.Format(@"
                    select u.Id, max(c.Id) as mId from 
                        Orchard.CRM.Core.Models.ActivityStreamRecord as c join c.User as u 
                        WHERE c.RelatedVersion.Id in ({0})
                        group by u.Id ", hql);

            var session = this.transactionManager.GetSession();
            var query = session
              .CreateQuery(hql)
              .SetResultTransformer(Transformers.AliasToEntityMap)
              .SetCacheable(false)
              .SetFirstResult(0)
              .SetMaxResults(count).List<IDictionary>();

            return query.Select(c => (int)c["0"]).ToList();
        }

        public int ActivityStreamRestrictedByGivenQueryCount(IHqlQuery[] contentQueries)
        {
            if (contentQueries == null) { throw new ArgumentNullException("contentQueries"); }

            var queries = new List<IHqlQuery>();
            foreach (var query in contentQueries)
            {
                queries.Add(this.ApplyContentPermissionFilter(query));
            }

            return this.Count(queries.ToArray());
        }

        public IEnumerable<ActivityStreamRecord> ActivityStreamVisibleByCurrentUser(int pageId, int pageSize)
        {
            if (this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
            {
                var items = this.repository.Table.OrderByDescending(c => c.Id).Skip(pageSize * pageId).Take(pageSize);
                return items;
            }

            var contentQuery = this.CreateBaseQuery();

            return this.RunQuery(new[] { contentQuery }, pageId, pageSize);
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            dynamic oldData = new ExpandoObject();
            var oldDataDictionary = oldData as IDictionary<string, object>;

            var currentUser = this.services.WorkContext.CurrentUser;
            foreach (var writer in this.activityStreamWriters)
            {
                oldDataDictionary[writer.Name] = writer.TakeSnapshot(contentItem);
            }

            return oldData;
        }

        public void WriteChangesToStreamActivity(ContentItem contentItem, dynamic snapshot, string mainStreamWriter)
        {
            this.WriteChangesToStreamActivity(contentItem, snapshot, false, mainStreamWriter);
        }

        public void WriteChangesToStreamActivity(ContentItem contentItem, dynamic snapshot, bool createdBySystem, string mainStreamWriter)
        {
            var currentUser = this.services.WorkContext.CurrentUser;
            int? userId = !createdBySystem && currentUser != null ? (int?)currentUser.Id : null;

            List<ActivityStreamChangeItem> changes = new List<ActivityStreamChangeItem>();
            List<ActivityStreamContentDescription> descriptions = new List<ActivityStreamContentDescription>();
            IDictionary<string, object> oldDataDictionary = snapshot != null ? snapshot as IDictionary<string, object> : null;

            foreach (var writer in this.activityStreamWriters)
            {
                var writerSnapshot = oldDataDictionary != null ? oldDataDictionary[writer.Name] : null;
                ActiviyStreamWriterContext context = new ActiviyStreamWriterContext(contentItem, writerSnapshot, currentUser);

                if (writer.CanApply(context))
                {
                    var changeDescriptions = writer.GetChangesDescriptions(context);

                    if (changeDescriptions != null)
                    {
                        changes.AddRange(changeDescriptions);
                    }

                    var activityContentDescription = writer.GetContentDescription(context);
                    if (activityContentDescription != null)
                    {
                        descriptions.Add(activityContentDescription);
                    }
                }
            }

            // description with highest weight
            ActivityStreamContentDescription contentDescription = null;

            if (!string.IsNullOrEmpty(mainStreamWriter))
            {
                contentDescription = descriptions.FirstOrDefault(c => c.WriterName == mainStreamWriter);
            }

            if (contentDescription == null)
            {
                contentDescription = descriptions
                     .OrderByDescending(c => c.Weight)
                     .FirstOrDefault();
            }

            // if it is an update, but noting is changed, then no activity record must be created
            if (snapshot != null && changes.Count == 0)
            {
                return;
            }

            string description = contentDescription.Description;

            if (changes.Count == 1)
            {
                description = string.Format(CultureInfo.InvariantCulture, "{1} - {0}", description, changes[0].Change).Trim();
                changes.Clear();
            }

            this.WriteChangesToStreamActivity(userId, contentItem.Id, contentItem.VersionRecord.Id, changes.ToArray(), description, contentDescription.RouteValues);
        }

        public void WriteChangesToStreamActivity(int? userId, int contentId, int versionId, ActivityStreamChangeItem[] changes, string contentDescription, RouteValueDictionary route)
        {
            this.WriteChangesToStreamActivity(userId, contentId, versionId, changes, contentDescription, route, true);
        }

        public void WriteChangesToStreamActivity(int? userId, int contentId, int versionId, ActivityStreamChangeItem[] changes, string contentDescription, RouteValueDictionary route, bool createLinkToTheChange)
        {
            var noneEmptyItems = changes.Where(c => !string.IsNullOrEmpty(c.Change));
            var changesInOriginalStreamRecord = noneEmptyItems.Where(c => c.RequireNewRecord == false).Select(c => c.Change).ToArray();
            var seconadaryChangesWhichNeedNewRecordPerChange = noneEmptyItems.Where(c => c.RequireNewRecord);

            string description = this.Encode(changesInOriginalStreamRecord, contentDescription, route, createLinkToTheChange);
            ActivityStreamRecord newRecord = new ActivityStreamRecord();
            newRecord.RelatedContent = new ContentItemRecord { Id = contentId };
            newRecord.RelatedVersion = new ContentItemVersionRecord { Id = versionId };
            newRecord.Description = description;
            newRecord.User = userId.HasValue ? new UserPartRecord { Id = userId.Value } : null;
            newRecord.CreationDateTime = DateTime.UtcNow;
            repository.Create(newRecord);

            foreach (var item in seconadaryChangesWhichNeedNewRecordPerChange)
            {
                description = this.Encode(new[] { item.Change }, contentDescription, null, createLinkToTheChange);
                ActivityStreamRecord secondaryChangeRecord = new ActivityStreamRecord();
                secondaryChangeRecord.RelatedContent = new ContentItemRecord { Id = item.RelatedContentId.HasValue ? item.RelatedContentId.Value : contentId };
                secondaryChangeRecord.RelatedVersion = new ContentItemVersionRecord { Id = item.RelatedContentVersionId.HasValue ? item.RelatedContentVersionId.Value : versionId };
                secondaryChangeRecord.Description = description;
                secondaryChangeRecord.User = userId.HasValue ? new UserPartRecord { Id = userId.Value } : null;
                secondaryChangeRecord.CreationDateTime = DateTime.UtcNow;
                repository.Create(secondaryChangeRecord);
            }

            repository.Flush();

            var contentItem = this.services.ContentManager.Get(contentId);

            if (contentItem != null)
            {
                workflowManager.TriggerEvent(NewActivityStreamActivity.ActivityStreamActivityName, contentItem, () => new Dictionary<string, object> { { "Content", contentItem }, { NewActivityStreamActivity.ActivityStreamRecordKey, newRecord } });
            }
        }

        public int ActivityStreamOfGivenUserVisibleByCurrentUserCount(int userId)
        {
            if (this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
            {
                return this.repository.Table.Where(c => c.User.Id == userId).Count();
            }

            var contentQuery = this.CreateBaseQuery();

            return this.Count(new[] { contentQuery }, userId);
        }

        public IEnumerable<ActivityStreamRecord> ActivityStreamOfGivenUserVisibleByCurrentUser(int userId, int pageId, int pageSize)
        {
            if (this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
            {
                var items = this.repository.Table.Where(c => c.User.Id == userId).OrderByDescending(c => c.Id).Skip(pageSize * pageId).Take(pageSize);
                return items;
            }

            var contentQuery = this.CreateBaseQuery();

            return this.RunQuery(new[] { contentQuery }, pageId, pageSize, userId);
        }

        public dynamic CreateModel(List<ActivityStreamRecord> items, int count, int page, int pageSize)
        {
            var session = this.transactionManager.GetSession();

            // set time zone
            items.ForEach(c =>
            {
                session.Evict(c);
            });

            dynamic model = new ExpandoObject();
            DateTime today = DateTime.UtcNow;

            // create pager
            var currentSite = this.services.WorkContext.CurrentSite;
            var pager = new Pager(currentSite, page, pageSize);
            model.Pager = this.services.New.Pager(pager).TotalItemCount(count);

            // contains the list of days, each day will contain the list of items in that day
            List<dynamic> dayModels = new List<dynamic>();
            model.Days = dayModels;
            var groupsByDay = items.GroupBy(c => c.CreationDateTime.Date).OrderByDescending(c => c.Key).ToList();

            foreach (var group in groupsByDay)
            {
                dynamic dayModel = new ExpandoObject();
                dayModels.Add(dayModel);
                dayModel.Date = group.Key;
                dayModel.Title = group.Key.Date == today.Date ? T("Today").Text : group.Key.ToString("dddd, MMMM, dd, yyyy", services.WorkContext.CurrentCultureInfo());

                List<dynamic> itemModels = new List<dynamic>();
                dayModel.Items = itemModels;
                foreach (var item in group)
                {
                    dynamic itemModel = this.CreateModel(item);

                    itemModels.Add(itemModel);
                }
            }

            return model;
        }

        public dynamic CreateModel(ActivityStreamRecord item)
        {
            var session = this.transactionManager.GetSession();

            session.Evict(item);
            item.CreationDateTime = CRMHelper.SetSiteTimeZone(this.services.WorkContext, item.CreationDateTime);

            dynamic itemModel = new ExpandoObject();
            dynamic description = JObject.Parse(item.Description);
            itemModel.Changes = description.Changes != null ?
                ((JArray)description.Changes).Select(c => c.ToString()).ToList() :
                new List<string>();
            RouteValueDictionary route = null;
            if (description.Route != null)
            {
                route = new RouteValueDictionary();
                foreach (var routeItem in description.Route)
                {
                    route.Add(routeItem.Name, routeItem.Value.ToString());
                }
            }
            else if (item.RelatedContent != null && description.CreateLinkToTheChange == true)
            {
                route = new RouteValueDictionary();
                route.Add("controller", "Item");
                route.Add("Action", "Display");
                route.Add("id", item.RelatedContent.Id);
                route.Add("area", "Contents");
            }

            itemModel.Route = route;
            itemModel.Id = item.RelatedContent.Id;
            itemModel.ActivityStreamId = item.Id;
            itemModel.ContentDescription = description.ContentDescription;
            itemModel.DateTime = item.CreationDateTime;

            if (item.User != null)
            {
                var user = this.basicDataService.GetOperatorOrCustomerUser(item.User.Id);
                itemModel.User = user;
                itemModel.UserId = item.User.Id;
                itemModel.UserFullName = user != null ? CRMHelper.GetFullNameOfUser(user) : item.User.UserName;
            }
            else
            {

                var user = _memebershipService.GetUser(services.WorkContext.CurrentSite.SuperUser);
                itemModel.User = user;
                itemModel.UserId = user.Id;
                itemModel.UserFullName = T("System").Text;
            }

            return itemModel;
        }

        private int Count(IHqlQuery[] contentQueries)
        {
            return this.Count(contentQueries, null);
        }

        private int Count(IHqlQuery[] contentQueries, int? userId)
        {
            List<string> queries = new List<string>();
            foreach (var contentQuery in contentQueries)
            {
                DefaultHqlQuery defaultQuery = contentQuery as DefaultHqlQuery;

                // This is for filling _form proprty of the DefaultHqlQuery, otherwise ToHql will not work
                var aliasFactory = new DefaultAliasFactory(defaultQuery);
                aliasFactory.ContentItem();

                // This condition is only for
                queries.Add(defaultQuery.ToHql(true));
            }

            queries = queries.Select(c => string.Format(CultureInfo.InvariantCulture, "c.RelatedVersion.Id in ({0})", c)).ToList();

            List<string> finalWhereConditions = new List<string> { string.Format(CultureInfo.InvariantCulture, "({0})", string.Join(" or ", queries)) };
            if (userId.HasValue)
            {
                finalWhereConditions.Add(string.Format(CultureInfo.InvariantCulture, "c.User.Id = {0}", userId.Value.ToString(CultureInfo.InvariantCulture)));
            }

            string condition = string.Join(" and ", finalWhereConditions);
            var mainHql = string.Format("select COUNT(c) from Orchard.CRM.Core.Models.ActivityStreamRecord as c WHERE {0}", condition);

            var session = this.transactionManager.GetSession();
            return Convert.ToInt32(session.CreateQuery(mainHql)
                           .SetCacheable(true)
                           .UniqueResult());
        }

        private IEnumerable<ActivityStreamRecord> RunQuery(IHqlQuery[] contentQueries, int pageId, int pageSize)
        {
            return this.RunQuery(contentQueries, pageId, pageSize, null);
        }

        private IEnumerable<ActivityStreamRecord> RunQuery(IHqlQuery[] contentQueries, int pageId, int pageSize, int? userId)
        {
            List<string> queries = new List<string>();
            foreach (var contentQuery in contentQueries)
            {
                DefaultHqlQuery defaultQuery = contentQuery as DefaultHqlQuery;

                // This is for filling _form proprty of the DefaultHqlQuery, otherwise ToHql will not work
                var aliasFactory = new DefaultAliasFactory(defaultQuery);
                aliasFactory.ContentItem();
                queries.Add(defaultQuery.ToHql(true));
            }

            queries = queries.Select(c => string.Format(CultureInfo.InvariantCulture, "c.RelatedVersion.Id in ({0})", c)).ToList();

            List<string> finalWhereConditions = new List<string> { string.Format(CultureInfo.InvariantCulture, "({0})", string.Join(" or ", queries)) };
            if (userId.HasValue)
            {
                finalWhereConditions.Add(string.Format(CultureInfo.InvariantCulture, "c.User.Id = {0}", userId.Value.ToString(CultureInfo.InvariantCulture)));
            }

            string condition = string.Join(" and ", finalWhereConditions);
            var mainHql = string.Format("select c from Orchard.CRM.Core.Models.ActivityStreamRecord as c WHERE {0} order by c.Id desc", condition);

            var session = this.transactionManager.GetSession();
            var query = session
              .CreateQuery(mainHql)
              .SetResultTransformer(Transformers.AliasToEntityMap)
              .SetCacheable(false)
              .SetFirstResult(pageId * pageSize)
              .SetMaxResults(pageSize).List<IDictionary>();

            return query.Select(c => (ActivityStreamRecord)c["0"]).ToList();
        }

        private string Encode(string[] changes, string contentDescription, RouteValueDictionary route, bool createLinkToTheChange)
        {
            dynamic model = new JObject();
            model.Changes = JToken.FromObject(changes);
            model.CreateLinkToTheChange = createLinkToTheChange;
            if (route != null)
            {
                Dictionary<string, object> routeValues = new Dictionary<string, object>(route);
                model.Route = JToken.FromObject(routeValues);
            }

            model.ContentDescription = contentDescription;

            return JsonConvert.SerializeObject(model);
        }

        private IHqlQuery CreateBaseQuery()
        {
            var contentQuery = this.services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);

            contentQuery = this.ApplyContentPermissionFilter(contentQuery);

            return contentQuery;
        }
    }
}
