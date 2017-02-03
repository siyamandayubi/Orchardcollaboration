using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.Localization;
using System.Globalization;
using Orchard.CRM.Core.Services;

namespace Orchard.CRM.Core.Providers.ActivityStream.Descriptors
{
    public class ProjectDescriptor : IContentItemDescriptor
    {
        private readonly IProjectService projectService;

        public ProjectDescriptor(IProjectService projectService)
        {
            this.projectService = projectService;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string GetDescription(IContent content)
        {
            var project = content.As<ProjectPart>();
            if (project == null)
            {
                return string.Empty;
            }

            project = this.projectService.GetProject(project.Record.Id);

            // means it is a new project
            if (project == null)
            {
                return T("New project").Text;
            }

            return T("Project - {0}", project.Record.Title).Text;
        }

        public bool CanApply(IContent content)
        {
            return content.As<ProjectPart>() != null;
        }
    }
}