using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Core.Containers.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Navigation.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;

namespace Orchard.CRM.Project.Handlers
{
    public class ProjectHandler : ContentHandler
    {
        public ProjectHandler(
            ISearchTicketService searchTicketService,
            IContentManager contentManager,
            IMenuService menuService,
            IRepository<ProjectPartRecord> repository,
            IRepository<AttachToProjectPartRecord> attachToProjectRepository,
            IExtendedProjectService projectService)
        {
            OnCreated<ProjectPart>((context, projectPart) =>
            {
                projectService.CreateProjectDependencies(projectPart);
            });

            OnRemoved<ProjectPart>((context, projectPart) =>
            {
                // Delete menu and menu widget
                var menu = menuService.GetMenu(projectPart.MenuId);
                if (menu != null)
                {
                    contentManager.Remove(menu.ContentItem);

                    var projectMenuWidget = projectService.GetProjectMenuWidget(projectPart.Id);
                    if (projectMenuWidget != null)
                    {
                        contentManager.Remove(projectMenuWidget);
                    }
                }

                // update all tickets who are attached to the project
                var attachToProjects = contentManager.HqlQuery<AttachToProjectPart>().Where(c => c.ContentPartRecord<AttachToProjectPartRecord>(), c => c.Eq("Project.Id", projectPart.Record.Id)).List();
                foreach (var attachToProject in attachToProjects)
                {
                    contentManager.Remove(attachToProject.ContentItem);
                }

            });

            OnLoaded<ProjectPart>((context, projectPart) =>
            {
                if (projectPart.RelatedItemsHaveBeenInitialized == false)
                {
                    projectService.CreateMilestoneAndBacklogForProject(projectPart);
                    projectPart.RelatedItemsHaveBeenInitialized = true;
                }

                // if the project doesn't have a menu, create a new one for it.
                if (projectPart.MenuId == default(int))
                {
                    projectService.CreateProjectMenu(projectPart);
                }
            });

            OnPublished<ProjectPart>((context, projectPart) =>
            {
                ProjectDashboardEditorPart dashboardPart = projectPart.As<ProjectDashboardEditorPart>();
                int[] portletIds = dashboardPart.PortletList ?? new int[] { };
                ContentItem projectDetail = contentManager
                    .Query()
                    .ForType(ContentTypes.ProjectDetailContentType)
                    .Where<AttachToProjectPartRecord>(c => c.Project.Id == projectPart.Id)
                    .Slice(1)
                    .FirstOrDefault();

                if (projectDetail == null)
                {
                    return;
                }

                // portlets
                var currentPortlets = contentManager.Query().Where<CommonPartRecord>(c => c.Container.Id == projectDetail.Id).List();

                var portletTemplates = contentManager.GetMany<ContentItem>(portletIds, VersionOptions.Published, new QueryHints());

                // add new portlets
                int position = -1;
                foreach (var portletId in portletIds)
                {
                    position++;
                    var currentPortlet = currentPortlets.FirstOrDefault(c => c.As<InfosetPart>().Retrieve<int>(FieldNames.ProjectDashboardPortletTemplateId) == portletId);
                    if (currentPortlet != null)
                    {
                        ContainablePart containablePart = currentPortlet.As<ContainablePart>();
                        containablePart.Position = position;
                        continue;
                    }

                    var portletTemplate = portletTemplates.FirstOrDefault(c => c.Id == portletId);

                    if (portletTemplate == null)
                    {
                        continue;
                    }

                    projectService.AddPortlet(projectDetail, portletTemplate, position);
                }

                // delete existing portlets that are not exist in the portletIds
                foreach (var portlet in currentPortlets)
                {
                    var templateId = portlet.As<InfosetPart>().Retrieve<int>(FieldNames.ProjectDashboardPortletTemplateId);

                    if (!portletIds.Contains(templateId))
                    {
                        contentManager.Remove(portlet);
                    }
                }
            });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            base.GetItemMetadata(context);

            if (context.ContentItem.ContentType == ContentTypes.ProjectDetailContentType)
            {
                context.Metadata.DisplayText = "Project Detail";

                AttachToProjectPart attachToProjectPart = context.ContentItem.As<AttachToProjectPart>();
                if (attachToProjectPart.Record.Project != null)
                {
                    context.Metadata.DisplayText += " " + attachToProjectPart.Record.Project.Title;
                }
            }
        }
    }
}