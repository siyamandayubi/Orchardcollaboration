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
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Tokens;
using Orchard.CRM.Core.Models;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Providers
{
    public class ProjectTokenProvider : Orchard.Tokens.ITokenProvider
    {
        private Localizer T { get; set; }
        private readonly IContentManager contentManager;

        public const string AttachToProjectKey = "AttachToProject";
        public const string ProjectKey = "Project";

        public ProjectTokenProvider(IContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context)
        {
            context.For(ProjectTokenProvider.AttachToProjectKey, T("AttachToProject"), T("AttachToProject"))
                 .Token("ProjectId", T("ProjectId"), T("ProjectId"));

            context.For(ProjectTokenProvider.ProjectKey, T("Project"), T("Project"))
          .Token("ProjectId", T("ProjectId"), T("ProjectId"));
        }

        public void Evaluate(EvaluateContext context)
        {
            // AttachToProject
            context.For(ProjectTokenProvider.AttachToProjectKey, () => this.GetPart<AttachToProjectPart>(context))
                .Token("ProjectId", c =>
                {
                    AttachToProjectPart attachToProjectPart = this.GetPart<AttachToProjectPart>(context);
                    var container = this.GetPart<CommonPart>(context).Container;
                    AttachToProjectPart attachToProjectPartInContainer = null;
                    if (container != null)
                    {
                        attachToProjectPartInContainer = container.As<AttachToProjectPart>();
                    }

                    if (attachToProjectPart != null)
                    {
                        return attachToProjectPart.Record.Project.Id.ToString(CultureInfo.InvariantCulture);
                    }

                    if (attachToProjectPartInContainer != null)
                    {
                        return attachToProjectPartInContainer.Record.Project.Id.ToString(CultureInfo.InvariantCulture);
                    }

                    return string.Empty;
                });

            // Project
            context.For(ProjectTokenProvider.ProjectKey, () => this.GetPart<ProjectPart>(context))
                .Token("ProjectId", c =>
                {
                    ProjectPart projectPart = this.GetPart<ProjectPart>(context);
                    AttachToProjectPart attachToProjectPart = this.GetPart<AttachToProjectPart>(context);
                    var container = this.GetPart<CommonPart>(context).Container;
                    AttachToProjectPart attachToProjectPartInContainer = null;
                    ProjectPart projectPartInContainer = null;

                    if (container != null)
                    {
                        attachToProjectPartInContainer = container.As<AttachToProjectPart>();
                        projectPartInContainer = container.As<ProjectPart>();
                    }

                    // Priority is as follows:
                    // first checking projectPart in the ContentItem
                    // then checking for availability of ProjectPart in the container
                    // If there is no ProjectPart, repeat the same process for AttachToProjectPart
                    if (projectPart != null)
                    {
                        return projectPart.Record.Id.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (projectPartInContainer != null)
                    {
                        return projectPartInContainer.Record.Id.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (attachToProjectPart != null)
                    {
                        return attachToProjectPart.Record.Project.Id.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (attachToProjectPartInContainer != null)
                    {
                        return attachToProjectPartInContainer.Record.Project.Id.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return string.Empty;
                    }
                });
        }

        private TPart GetPart<TPart>(EvaluateContext context)
         where TPart : ContentPart
        {
            ContentItem contentItem = (ContentItem)context.Data["Content"];
            if (contentItem == null)
            {
                return null;
            }

            return contentItem.As<TPart>();
        }
    }
}