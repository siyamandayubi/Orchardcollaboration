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
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public class BusinessUnitService : IBusinessUnitService
    {
        private IRepository<BusinessUnitPartRecord> businessUnitPartRecordRepository;
        private IBasicDataService basicDataService;
        private IOrchardServices orchardServices;

        public BusinessUnitService(
            IBasicDataService basicDataService,
            IRepository<BusinessUnitPartRecord> businessUnitPartRecordRepository,
            IOrchardServices orchardServices
            )
        {
            this.basicDataService = basicDataService;
            this.orchardServices = orchardServices;
            this.businessUnitPartRecordRepository = businessUnitPartRecordRepository;
        }

        public void Fill(Collection<BusinessUnitViewModel> target, bool restrictToUserPermissions)
        {
            List<BusinessUnitPartRecord> businessUnits = null;
            List<TeamPartRecord> teams = null;

            var businessUnitsContentItems = this.basicDataService.GetBusinessUnits().ToList();
            var teamContentItems = this.basicDataService.GetTeams();
            teams = teamContentItems.Select(c => c.As<TeamPart>().Record).ToList();
            businessUnits = businessUnitsContentItems.Select(c => c.As<BusinessUnitPart>().Record).ToList();
            
            if (restrictToUserPermissions)
            {
                int userId = this.orchardServices.WorkContext.CurrentUser.Id;
                var userBusinessUnits = this.basicDataService.GetBusinessUnitMembers().Where(c => c.UserPartRecord.Id == userId).ToList();
                businessUnits = businessUnits.Where(c => userBusinessUnits.Any(d => d.BusinessUnitPartRecord.Id == c.Id)).ToList();

                var userTeams = this.basicDataService.GetTeamMembers().Where(c => c.UserPartRecord.Id == userId).ToList();
                teams = teams.Where(c => userTeams.Any(d => d.TeamPartRecord.Id == c.Id)).ToList();
            }

            // organize the businessUnits in a tree structure including the teams
            Queue<BusinessUnitViewModel> queue = new Queue<BusinessUnitViewModel>();

            foreach (var root in businessUnits.Where(c => c.Parent == null))
            {
                BusinessUnitViewModel businessUnitViewModel = this.Convert(root);
                target.Add(businessUnitViewModel);
                queue.Enqueue(businessUnitViewModel);
            }

            while (queue.Count > 0)
            {
                var businessUnitViewModel = queue.Dequeue();
                var childRecords = businessUnits.Where(c => c.Parent != null && c.Parent.Id == businessUnitViewModel.BusinessUnitId).ToList();

                foreach (var childRecord in childRecords)
                {
                    var childModel = this.Convert(childRecord);
                    businessUnitViewModel.BusinessUnits.Add(childModel);
                    queue.Enqueue(childModel);
                }

                var childTeams = teams.Where(c => c.BusinessUnitPartRecord.Id == businessUnitViewModel.BusinessUnitId);

                foreach (var team in childTeams)
                {
                    var teamModel = this.Convert(team);
                    businessUnitViewModel.Teams.Add(teamModel);
                }
            }
        }

        private TeamViewModel Convert(TeamPartRecord teamRecord)
        {
            TeamViewModel teamViewModel = new TeamViewModel
            {
                TeamId = teamRecord.Id,
                BusinessUnitId = teamRecord.BusinessUnitPartRecord != null ? (int?)teamRecord.BusinessUnitPartRecord.Id : null,
                Name = teamRecord.Name,
                Description = teamRecord.Description
            };

            return teamViewModel;
        }

        private BusinessUnitViewModel Convert(BusinessUnitPartRecord record)
        {
            BusinessUnitViewModel businessUnitViewModel = new BusinessUnitViewModel
            {
                BusinessUnitId = record.Id,
                Name = record.Name,
                Description = record.Description
            };

            return businessUnitViewModel;
        }
    }
}