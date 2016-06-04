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
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.Settings;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Drivers
{
    public class AttachToProjectDriver : ContentPartDriver<AttachToProjectPart>
    {
        private readonly IOrchardServices services;
        private readonly IProjectService projectService;
        private readonly IHelperService helperService;

        public AttachToProjectDriver(IOrchardServices services, IProjectService projectService, IHelperService helperService)
        {
            this.helperService = helperService;
            this.projectService = projectService;
            this.services = services;
        }

        protected override DriverResult Display(AttachToProjectPart part, string displayType, dynamic shapeHelper)
        {
            var settings = part.TypePartDefinition.Settings.GetModel<AttachToProjectPartSettings>();

            if (settings.HiddenInDisplayModel)
            {
                return null;
            }

            if (part.Record.Project != null)
            {
                return ContentShape("Parts_AttachToProject",
                     () => shapeHelper.Parts_AttachToProject(
                         Model: part));
            }
            else
            {
                return null;
            }
        }

        protected override DriverResult Editor(AttachToProjectPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            EditAttachToViewModel model = new EditAttachToViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            // check user has access to old project
            if (part.Record.Project != null)
            {
                var oldProject = this.projectService.GetProject(part.Record.Project.Id);
                if (oldProject == null)
                {
                    return null;
                }
            }

            if (model.SelectedId == null)
            {
                part.Record.Project = null;
            }
            else
            {
                // check user has access to new project
                var newProject = this.projectService.GetProject(model.SelectedId.Value);

                if (newProject == null)
                {
                    return null;
                }

                part.Record.Project = new ProjectPartRecord { Id = model.SelectedId.Value };
            }

            return this.Editor(part, shapeHelper);
        }

        protected int? GetParentProjectId(AttachToProjectPart part)
        {
            // Check whether the given contentItem is a Ticket
            int? parentProjectId = null;
            TicketPart ticketPart = part.As<TicketPart>();
            if (ticketPart != null && ticketPart.Record.Parent != null)
            {
                // if the parent has a milestone, then the current ticket belongs to that milestone too
                var parent = this.services.ContentManager.Get(ticketPart.Record.Parent.Id, VersionOptions.Published, new QueryHints().ExpandParts<AttachToProjectPart>());
                var parentAttachToProject = parent.As<AttachToProjectPart>();
                parentProjectId = parentAttachToProject.Record.Project != null ? (int?)parentAttachToProject.Record.Project.Id : null;
            }

            return parentProjectId;
        }

        protected override DriverResult Editor(AttachToProjectPart part, dynamic shapeHelper)
        {
            var settings = part.TypePartDefinition.Settings.GetModel<AttachToProjectPartSettings>();

            int? projectId = null;
            int? parentProjectId = this.GetParentProjectId(part);

            // if it is Create Mode
            if (part.Record.Project == null)
            {
                projectId = this.projectService.GetProjectIdFromQueryString();

                if (projectId != null)
                {
                    var projectPart = this.projectService.GetProject(projectId.Value);

                    if (projectPart != null)
                    {
                        part.Record.Project = projectPart.Record;
                    }
                }
            }
            else
            {
                projectId = part.Record.Project != null ? (int?)part.Record.Project.Id : null;
            }

            // For some contents, we don't want to represent the project selection, 
            if (!settings.HiddenInEditMode && parentProjectId == null)
            {
                var projects = this.projectService.GetProjects(null).Select(c => c.As<ProjectPart>()).ToList();

                if (projects.Count == 0)
                {
                    return null;
                }

                // user doesn't have access to the project
                if (projectId.HasValue && !projects.Any(c => c.Id == projectId.Value))
                {
                    return null;
                }

                EditAttachToViewModel model = new EditAttachToViewModel();
                Converter.Fill(model.Items, projects);

                if (projectId.HasValue)
                {
                    var selectedItem = model.Items.FirstOrDefault(c => c.Value == projectId.Value.ToString(CultureInfo.InvariantCulture));
                    if (selectedItem != null)
                    {
                        selectedItem.Selected = true;
                    }
                }

                return ContentShape("Parts_AttachToProject_Edit",
                       () => shapeHelper.EditorTemplate(
                           TemplateName: "Parts/AttachToProject",
                           Model: model,
                           Prefix: Prefix));
            }
            else
            {
                return ContentShape("Parts_AttachToProject_HiddenField_Edit",
                       () => shapeHelper.EditorTemplate(
                           TemplateName: "Parts/AttachToProject.HiddenField",
                           Model: projectId,
                           Prefix: Prefix));
            }
        }
    }
}