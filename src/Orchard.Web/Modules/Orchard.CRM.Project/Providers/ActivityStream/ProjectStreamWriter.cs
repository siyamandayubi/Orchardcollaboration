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

using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.ActivityStream;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using System.Web;
using System.Globalization;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using System.Web.Routing;
using Orchard.CRM.Core.Services;

namespace Orchard.CRM.Project.Providers.ActivityStream
{
    public class ProjectStreamWriter : IActivityStreamWriter
    {
        private readonly IOrchardServices services;
        private readonly IProjectService projectService;
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;

        public ProjectStreamWriter(IOrchardServices services, IContentItemDescriptorManager contentItemDescriptorManager, IProjectService projectService)
        {
            this.projectService = projectService;
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.services = services;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            return context.ContentItem.As<ProjectPart>() != null;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            List<string> changes = new List<string>();

            ProjectPartRecord old = context.Snapshot != null ? (context.Snapshot as ProjectPartRecord) : null;
            ProjectPartRecord newValue = (context.ContentItem.As<ProjectPart>()).Record;

            if (old == null)
            {
                string change = T("Project is created").Text;
                return new[] { new ActivityStreamChangeItem(change) };
            }

            // change Title
            if (old.Title != newValue.Title)
            {
                string newTitleString = !string.IsNullOrEmpty(newValue.Title) ? newValue.Title : this.T("Empty").Text;
                newTitleString = string.Format(
                    CultureInfo.CurrentUICulture,
                    T("changed the Title to: '{0}'").Text,
                    newTitleString);

                changes.Add(newTitleString);
            }

            // change Description
            if (old.Description != newValue.Description)
            {
                string newDescriptionString = !string.IsNullOrEmpty(newValue.Description) ? newValue.Description : this.T("Empty").Text;
                newDescriptionString = T("changed the Description").Text;

                changes.Add(newDescriptionString);
            }

            return changes.Select(c => new ActivityStreamChangeItem(c));
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            var projectPart = context.ContentItem.As<ProjectPart>();

            string projectDescription = contentItemDescriptorManager.GetDescription(context.ContentItem);

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("action", "Display");
            routeValueDictionary.Add("controller", "Project");
            routeValueDictionary.Add("area", "Orchard.CRM.Project");
            routeValueDictionary.Add("id", context.ContentItem.Id);

            ActivityStreamContentDescription returnValue = new ActivityStreamContentDescription(StreamWriters.ProjectStreamWriter) { Weight = 1, RouteValues = routeValueDictionary };

            var originalProject = this.projectService.GetProject(projectPart.Record.Id);

            // new project
            if (originalProject == null)
            {
                returnValue.Description = T("On new project").Text;
            }
            else if (context.Snapshot == null)
            {
                returnValue.Description = T("Creates the '{0}'", projectDescription).Text;
            }
            else
            {
                returnValue.Description = string.Format("{0} {1}", this.T("on ").Text, projectDescription);
            }

            return returnValue;
        }

        public string Name
        {
            get { return "Project"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            var part = contentItem.As<ProjectPart>();

            if (part == null)
            {
                return null;
            }

            ProjectPartRecord oldData = new ProjectPartRecord();
            oldData.Title = part.Record.Title;
            oldData.Description = part.Record.Description;

            return oldData;
        }
    }
}