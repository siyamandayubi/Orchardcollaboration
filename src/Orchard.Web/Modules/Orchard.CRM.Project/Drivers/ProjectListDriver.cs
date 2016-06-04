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

using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Drivers
{
    public class ProjectListDriver : ContentPartDriver<ProjectListPart>
    {
        private readonly IProjectService projectService;
        private readonly IOrchardServices services;
        protected readonly ICRMContentOwnershipService contentOwnershipService;
        private readonly IHelperService helperService;
        public Localizer T { get; set; }

        public ProjectListDriver(
            IHelperService helperService,
            IProjectService projectService,
            ICRMContentOwnershipService contentOwnershipService,
            IOrchardServices services)
        {
            this.helperService = helperService;
            this.services = services;
            this.contentOwnershipService = contentOwnershipService;
            this.projectService = projectService;
            this.T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ProjectListPart part, string displayType, dynamic shapeHelper)
        {
            if (!this.contentOwnershipService.CurrentUserCanViewContent(part))
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to view the item"));
            }

            var pager = this.helperService.ReterivePagerFromQueryString();

            var projects = this.projectService.GetProjects(pager);
            int count = this.projectService.GetProjectsCount();

            var pagerShape = shapeHelper.Pager(pager).PagerId("page");
            pagerShape.TotalItemCount(count);
            
            ProjectListViewModel model = new ProjectListViewModel
            {
                CanCreateProject =  this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission),
                Pager = pagerShape
            };

            foreach (var project in projects)
            {
                model.Projects.Add(this.services.ContentManager.BuildDisplay(project, "Summary"));
            }

            return ContentShape("Parts_ProjectList",
                  () => shapeHelper.Parts_ProjectList(
                      Model: model
                      ));
        }   
    }
}