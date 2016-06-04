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

using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.Projections.Models;
using System;
using System.Collections.Generic;
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

        public IEnumerable<ContentItem> GetMilestoneItems(int milestoneId)
        {
            var query = this.services.ContentManager.HqlQuery();

            query = this.ApplyContentPermissionFilter(query);
            
            query = query.Where(c => c.ContentPartRecord<AttachToMilestonePartRecord>(), c => c.Eq("MilestoneId", milestoneId));

            return query.List();
        }
    }
}