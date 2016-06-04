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
using Orchard.CRM.Project.Commands;
using Orchard.Data;
using Orchard.Projections.Models;
using Orchard.Reporting.Models;
using Orchard.Workflows.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Services
{
    public class ProjectSetup : IProjectSetup
    {
        private ProjectCommand projectCommand;
        private PortletCommand portletCommand;

        private readonly IExtendedProjectService projectService;
        private readonly IOrchardServices services;

        public ProjectSetup(
            IExtendedProjectService projectService,
            IOrchardServices services,
            IRepository<SortCriterionRecord> sortRepository,
            IRepository<EmailTemplateRecord> emailTemplatesRepository,
            IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository,
            IRepository<ActivityRecord> activityRepository,
            IRepository<TransitionRecord> transitionRepository,
            IContentManager contentManager,
            IRepository<FilterRecord> filterRepository,
            IRepository<LayoutRecord> layoutRepository,
            IRepository<FilterGroupRecord> filterGroupRepository,
            IRepository<ReportRecord> reportRepository,
            IBasicDataService basicDataService)
        {
            this.projectService = projectService;
            this.services = services;

            this.projectCommand = new ProjectCommand(sortRepository, emailTemplatesRepository, workflowDefinitionRepository, activityRepository, transitionRepository, reportRepository, layoutRepository, contentManager, filterRepository, filterGroupRepository, basicDataService);
            this.portletCommand = new PortletCommand(services, reportRepository, layoutRepository);
        }

        public void Setup()
        {
            this.projectCommand.CreateSendEmailToFollowersWorkflow();
            this.projectCommand.CreateProjectActivityStreamQuery();
            this.projectCommand.CreateWikiActivityStreamQuery();
            this.projectCommand.CreateProjectTicketsQuery();
            this.projectCommand.CreateProjectUnassignedTicketsQuery();
            this.projectCommand.CreateProjectDiscussionQuery();
            this.projectCommand.CreateProjectMilestonesQuery();
            this.projectCommand.CreateProjectDiscussionSummaryQuery();
            this.projectCommand.CreateProjectTicketsGroupByStatusReport();
        }

        public void Setup2()
        {
            this.projectCommand.CreateOverdueProjectTicketsQuery();
            this.projectCommand.CreateProjectWikiesSummaryQuery();
            this.projectCommand.CreateHighPriorityProjectTicketsQuery();
            this.projectCommand.CreateNotClosedProjectTicketsQuery();
            this.projectCommand.CreateProjectNotTicketsGroupByPriorityReport();
            this.projectCommand.CreateProjectOverdueTicketsGroupByTicketTypeReport();
            this.portletCommand.Create();
        }

        public void Setup3()
        {
            this.projectCommand.CreateProjectMilestonesQuery();
        }
    }
}