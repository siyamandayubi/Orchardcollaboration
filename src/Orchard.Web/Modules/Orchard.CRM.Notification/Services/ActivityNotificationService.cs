using Orchard.CRM.Core.Services;
using Orchard.CRM.Notification.Models;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.ContentManagement;
using System.Globalization;
using System.Collections;
using NHibernate.Transform;
using Orchard.Core.Common.Models;
using System.Dynamic;

namespace Orchard.CRM.Notification.Services
{
    public class ActivityNotificationService : IActivityNotificationService
    {
        private readonly IOrchardServices services;
        private readonly IActivityStreamService activityStreamService;
        private readonly IRepository<UserContentVisitRecord> userContentVisitRepository;
        protected readonly IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort;
        protected readonly ITransactionManager transactionManager;

        public ActivityNotificationService(
            ITransactionManager transactionManager,
            IOrchardServices services,
            IActivityStreamService activityStreamService,
            IRepository<UserContentVisitRecord> userContentVisitRepository,
            IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort)
        {
            this.transactionManager = transactionManager;
            this.projectionManagerWithDynamicSort = projectionManagerWithDynamicSort;
            this.activityStreamService = activityStreamService;
            this.services = services;
            this.userContentVisitRepository = userContentVisitRepository;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ContentItem GetLatestCRMNotificationListItem()
        {
            var contentItem = this
                .services
                .ContentManager
                .HqlQuery()
                .ForType(Consts.CRMNotificationListType)
                .OrderBy(c => c.ContentPartRecord<CommonPartRecord>(), c => c.Desc("Id"))
                .Slice(0, 1)
                .ToList()
                .FirstOrDefault();

            if (contentItem == null)
            {
                contentItem = this.services.ContentManager.New(Consts.CRMNotificationListType);
                this.services.ContentManager.Create(contentItem);
                this.services.ContentManager.Publish(contentItem);
            }

            return contentItem;
        }

        public int NewItemsCount()
        {
            if (this.services.WorkContext.CurrentUser == null)
            {
                throw new OrchardSecurityException(T("You don't have authorization to run the query"));
            }

            int userId = this.services.WorkContext.CurrentUser.Id;
            int lastActivityId = 0;
            var queries = this.GetQueries(out lastActivityId);

            if (lastActivityId == 0)
            {
                return 0;
            }
            return this.Count(queries, userId, lastActivityId);
        }

        public IEnumerable<ActivityStreamRecord> Notifications(int pageId, int pageSize)
        {
            int userId = this.services.WorkContext.CurrentUser.Id;
            int lastActivityId = 0;
            var queries = this.GetQueries(out lastActivityId);

            if (lastActivityId == 0)
            {
                return new List<ActivityStreamRecord>();
            }

            return RunQuery(queries, pageId, pageSize, userId, lastActivityId);
        }

        public void UpdateLastVisitActivity(int activityStreamId)
        {
            if (this.services.WorkContext.CurrentUser == null)
            {
                throw new OrchardSecurityException(T("You don't have authorization to run the query"));
            }

            int userId = this.services.WorkContext.CurrentUser.Id;
            var userVisitRecord = userContentVisitRepository.Table.FirstOrDefault(c => c.User.Id == userId);

            if (userVisitRecord == null)
            {
                userVisitRecord = new UserContentVisitRecord
                {
                    User = new UserPartRecord() { Id = userId },
                    LastVisitedActivityStreamId = activityStreamId,
                    LastVisitTime = DateTime.UtcNow
                };

                this.userContentVisitRepository.Create(userVisitRecord);
                this.userContentVisitRepository.Flush();
            }
            else
            {
                userVisitRecord.LastVisitedActivityStreamId = activityStreamId;
                userVisitRecord.LastVisitTime = DateTime.UtcNow;
                this.userContentVisitRepository.Flush();
            }
        }

        protected IHqlQuery ApplyContentPermissionFilter(IHqlQuery contentQuery)
        {
            // TeamIds of the search
            var teams = new List<int>();

            // BusinessUnitIds of the search
            List<int> businessUnits = new List<int>();

            // Ids of the searched users
            List<int> users = new List<int>();

            // restrict the list for none admin users
            if (!this.services.Authorizer.Authorize(Orchard.CRM.Core.Permissions.AdvancedOperatorPermission))
            {
                int? accessType = ContentItemPermissionAccessTypes.Assignee;
                this.RestrictLists(ref teams, ref users, ref businessUnits);

                dynamic permissionsState = new ExpandoObject();
                permissionsState.Users = users;
                permissionsState.Teams = teams;
                permissionsState.BusinessUnits = businessUnits;
                permissionsState.AccessType = ContentItemPermissionAccessTypes.Assignee;

                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.AnySelectedUserTeamBusinessUnit, permissionsState);
            }

            return contentQuery;
        }

        private void RestrictLists(ref List<int> teams, ref List<int> users, ref List<int> businessUnits)
        {
            int userId = this.services.WorkContext.CurrentUser.Id;

            var userBusinessUnits = this.services.ContentManager
              .Query()
              .ForType("BusinessUnitMember")
              .ForPart<BusinessUnitMemberPart>()
              .Where<BusinessUnitMemberPartRecord>(c => c.UserPartRecord.Id == userId)
              .List()
              .ToList();


            var userTeams = this.services.ContentManager
                .Query()
                .ForType("TeamMember")
                .ForPart<TeamMemberPart>()
                .Where<TeamMemberPartRecord>(c => c.UserPartRecord.Id == userId)
                .List()
                .ToList();

            // Restrict to teams and businessUnits of the user
            teams = teams.Where(c => userTeams.Count(d => d.Record.TeamPartRecord.Id == c) > 0).ToList();
            businessUnits = businessUnits.Where(c => userBusinessUnits.Count(d => d.Record.BusinessUnitPartRecord.Id == c) > 0).ToList();
            users = users.Where(c => c == userId).ToList();

            if (teams.Count == 0 && businessUnits.Count == 0 && users.Count == 0)
            {
                users.Add(userId);
                users = users.Distinct().ToList();
                var userTeamIds = userTeams.Select(c => c.Record.TeamPartRecord.Id);
                teams.AddRange(userTeamIds);
                teams = teams.Distinct().ToList();

                var userBusinessUnitIds = userBusinessUnits.Select(c => c.Record.BusinessUnitPartRecord.Id);
                businessUnits.AddRange(userBusinessUnitIds);
                businessUnits = businessUnits.Distinct().ToList();
            }
        }

        private IHqlQuery[] GetQueries(out int lastActivityId)
        {
            int userId = this.services.WorkContext.CurrentUser.Id;
            var userVisitRecord = userContentVisitRepository.Table.FirstOrDefault(c => c.User.Id == userId);

            var contentQuery = this.services.ContentManager.HqlQuery();
            contentQuery = this.ApplyContentPermissionFilter(contentQuery);
            if (userVisitRecord == null)
            {
                var lastActivities = this.activityStreamService.ActivityStreamRestrictedByGivenQuery(new[] { contentQuery }, 0, 1).ToList();

                if (lastActivities.Count > 0)
                {
                    userVisitRecord = new UserContentVisitRecord
                    {
                        User = new UserPartRecord() { Id = userId },
                        LastVisitedActivityStreamId = lastActivities[0].Id,
                        LastVisitTime = DateTime.UtcNow
                    };

                    this.userContentVisitRepository.Create(userVisitRecord);
                    this.userContentVisitRepository.Flush();
                }

                lastActivityId = 0;
            }
            else
            {
                lastActivityId = userVisitRecord.LastVisitedActivityStreamId;
            }

            return new[] { contentQuery };
        }

        private int Count(IHqlQuery[] contentQueries, int? userId, int minimumActivityStreamId)
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
                finalWhereConditions.Add(string.Format(CultureInfo.InvariantCulture, "c.User.Id <> {0}", userId.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (minimumActivityStreamId > 0)
            {
                finalWhereConditions.Add(string.Format(CultureInfo.InvariantCulture, "c.Id > {0}", minimumActivityStreamId.ToString(CultureInfo.InvariantCulture))); ;
            }

            string condition = string.Join(" and ", finalWhereConditions);
            var mainHql = string.Format("select COUNT(c) from Orchard.CRM.Core.Models.ActivityStreamRecord as c WHERE {0}", condition);

            var session = this.transactionManager.GetSession();
            return Convert.ToInt32(session.CreateQuery(mainHql)
                           .SetCacheable(true)
                           .UniqueResult());
        }

        private IEnumerable<ActivityStreamRecord> RunQuery(IHqlQuery[] contentQueries, int pageId, int pageSize, int? userId, int? minimumActivityStreamId)
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
                finalWhereConditions.Add(string.Format(CultureInfo.InvariantCulture, "c.User.Id <> {0}", userId.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (minimumActivityStreamId.HasValue)
            {
                finalWhereConditions.Add(string.Format(CultureInfo.InvariantCulture, "c.Id > {0}", minimumActivityStreamId.Value.ToString(CultureInfo.InvariantCulture)));
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
    }
}