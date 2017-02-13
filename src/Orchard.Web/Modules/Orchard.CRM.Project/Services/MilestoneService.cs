using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.Projections.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Services
{
    public class MilestoneService : BaseService, IMilestoneService
    {
        private readonly IOrchardServices services;
        public MilestoneService(IOrchardServices services, IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort)
            : base(services, projectionManagerWithDynamicSort)
        {
            this.services = services;
        }

        public ContentItem GetMilestone(int id)
        {
            return this.services.ContentManager.Get(id);
        }

        public ContentItem GerProjectBacklog(int projectId)
        {
            var query = this.services.ContentManager.HqlQuery().ForType(ContentTypes.MilestoneContentType);

            // attach to project
            query = query.Where(c => c.ContentPartRecord<AttachToProjectPartRecord>(), c => c.Eq("Project.Id", projectId));

            // not closed milestones
            query = query.Where(c => c.ContentPartRecord<MilestonePartRecord>(), c => c.Eq("IsBacklog", true));

            return query.List().FirstOrDefault();
        }

        public IEnumerable<ContentItem> GetOpenMilestones(int projectId)
        {
            var query = this.services.ContentManager.HqlQuery().ForType(ContentTypes.MilestoneContentType);

            query = this.ApplyContentPermissionFilter(query);

            // attach to project
            query = query.Where(c => c.ContentPartRecord<AttachToProjectPartRecord>(), c => c.Eq("Project.Id", projectId));

            // not closed milestones
            query = query.Where(c => c.ContentPartRecord<MilestonePartRecord>(), c => c.Eq("IsClosed", false));

            return query.List();
        }

        public IEnumerable<ContentItem> GetMilestoneItems(int milestoneId, bool onlyNoneCompleted)
        {
            var query = this.services.ContentManager.HqlQuery();

            query = this.ApplyContentPermissionFilter(query);

            if (onlyNoneCompleted)
            {
                dynamic state = new JObject();
                state.StatusType_Id = StatusRecord.ClosedStatus.ToString(CultureInfo.InvariantCulture);
                state.NotEqual = true.ToString(CultureInfo.InvariantCulture);
                query = projectionManagerWithDynamicSort.ApplyFilter(query, TicketFieldsFilter.CategoryName, TicketFieldsFilter.StatusTypeFilter, state);
            }

            query = query.Where(c => c.ContentPartRecord<AttachToMilestonePartRecord>(), c => c.Eq("MilestoneId", milestoneId));

            return query.List();
        }
    }
}