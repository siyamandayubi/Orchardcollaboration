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
using Orchard.CRM.Core;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public class BaseService
    {
        protected readonly IOrchardServices services;
        protected readonly IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort;

        public BaseService(IOrchardServices services, IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort)
        {
            this.projectionManagerWithDynamicSort = projectionManagerWithDynamicSort;
            this.services = services;
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
            if (!this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
            {
                int? accessType = null;
                this.RestrictLists(ref teams, ref users, ref businessUnits);

                dynamic permissionsState = new
                {
                    Users = users,
                    Teams = teams,
                    BusinessUnits = businessUnits,
                    AccessType = accessType
                };

                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.AnySelectedUserTeamBusinessUnit, permissionsState);
            }

            return contentQuery;
        }

        private void RestrictLists(ref List<int> teams, ref List<int> users, ref List<int> businessUnits)
        {
            int userId = this.services.WorkContext.CurrentUser.Id;

            if (!this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
            {
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
        }
    }
}