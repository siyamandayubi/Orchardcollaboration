using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.ViewModels;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.CRM.Core;
using Orchard.Security;
using System.Web.Mvc;
using System.Globalization;

namespace Orchard.CRM.Project.Drivers
{
    public class ProjectItemPermissionDriver : ContentPartDriver<ProjectItemPermissionsPart>
    {
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly IContentOwnershipHelper contentOwnershipHelper;
        private readonly IProjectService projectService;
        private readonly IOrchardServices orchardServices;

        public ProjectItemPermissionDriver(
            IOrchardServices orchardServices,
            ICRMContentOwnershipService crmContentOwnershipService,
            IContentOwnershipHelper contentOwnershipHelper,
            IProjectService projectService)
        {
            this.orchardServices = orchardServices;
            this.projectService = projectService;
            this.contentOwnershipHelper = contentOwnershipHelper;
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(ProjectItemPermissionsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            // only applicable in the Creation mode
            if (part.ContentItem.VersionRecord.Published == true || part.ContentItem.Record.Versions.Count > 1)
            {
                return null;
            }

            ProjectPermissionEditPostViewModel model = new ProjectPermissionEditPostViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            if (model.ProjectId == null)
            {
                updater.AddModelError(Prefix + ".ProjectId", T("ProjectId is a required field"));
                return this.Editor(part, shapeHelper);
            }

            var project = this.projectService.GetProject(model.ProjectId.Value);

            if (project == null)
            {
                updater.AddModelError(Prefix + ".ProjectId", T("There is no project with the given Id"));
                return this.Editor(part, shapeHelper);
            }

            bool isOperatorOrCustomer = this.crmContentOwnershipService.IsCurrentUserOperator() || this.crmContentOwnershipService.IsCurrentUserCustomer();
            int currentUserId = this.orchardServices.WorkContext.CurrentUser.Id;

            EditContentPermissionViewModel editContentPermissionViewModel = null;
            if (model.VisibleToAll)
            {
                var projectContentPermissionPart = project.As<ContentItemPermissionPart>();

                if (projectContentPermissionPart == null)
                {
                    throw new System.ArgumentNullException("Project is not associated with ContentItemPermissionPart");
                }

                editContentPermissionViewModel = new EditContentPermissionViewModel
                {
                    AccessType = ContentItemPermissionAccessTypes.SharedForView,
                    ContentIds = new[] { part.ContentItem.Id },
                    RemoveOldPermission = false
                };

                var items = projectContentPermissionPart.Record.Items;
                if (items != null && items.Count > 0)
                {
                    var projectUsers = items.Where(c => c.User != null).Select(c => c.User).ToList();

                    // if current user is an operator or customer, then he/she will have Owner/edit access,so he/she must be removed from the list
                    // in order to prevent rewriting his privelege on the item
                    projectUsers = isOperatorOrCustomer ? projectUsers.Where(c => c.Id != currentUserId).ToList() : projectUsers;
                    editContentPermissionViewModel.Targets.AddRange(projectUsers.Select(c => new TargetContentItemPermissionViewModel
                    {
                        UserId = c.Id,
                        Checked = true
                    }));

                    var projectGroups = items.Where(c => c.BusinessUnit != null).Select(c => c.BusinessUnit).ToList();
                    editContentPermissionViewModel.Targets.AddRange(projectGroups.Select(c => new TargetContentItemPermissionViewModel
                    {
                        BusinessUnitId = c.Id,
                        Checked = true
                    }));
                }
            }
            else
            {
                editContentPermissionViewModel = this.Convert(model, part.ContentItem.Id, ContentItemPermissionAccessTypes.SharedForView);
            }

            this.contentOwnershipHelper.Update(editContentPermissionViewModel, new List<ContentItem>() { part.ContentItem });

            return null;
        }

        protected override DriverResult Editor(ProjectItemPermissionsPart part, dynamic shapeHelper)
        {
            // only visible in the Create mode
            if (part.Id != default(int))
            {
                return null;
            }
            
            var projectId = this.projectService.GetProjectIdFromQueryString();

            var model = this.contentOwnershipHelper.CreateModel();

            List<IUser> customers = new List<IUser>();
            if (projectId != null)
            {
                var project = this.projectService.GetProject(projectId.Value);
                var temp = this.contentOwnershipHelper.GetCustomersWhoHaveAccessToThisContent(project);
                customers = customers.Union(temp.Where(c => !customers.Any(d => d.Id == c.Id)).ToList()).ToList();
                this.contentOwnershipHelper.RestrictToPeopleWhoHavePermissionInGivenItem(model, project.ContentItem);
            }

            model.Customers.AddRange(customers.Select(c => new SelectListItem { Text = CRMHelper.GetFullNameOfUser(c), Value = c.Id.ToString(CultureInfo.InvariantCulture) }));
            
            model.Tag = projectId;
            return ContentShape("Parts_ProjectItemPermissions_Edit",
                        () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/ProjectItemPermissions",
                        Model: model,
                        Prefix: Prefix));
        }

        protected EditContentPermissionViewModel Convert(ProjectPermissionEditPostViewModel inputModel, int contentId, byte accessType)
        {
            EditContentPermissionViewModel model = new EditContentPermissionViewModel
            {
                ContentIds = new[] { contentId },
                AccessType = accessType,
                RemoveOldPermission = false,
            };

            bool isOperatorOrCustomer = this.crmContentOwnershipService.IsCurrentUserOperator() || this.crmContentOwnershipService.IsCurrentUserCustomer();
            int currentUserId = this.orchardServices.WorkContext.CurrentUser.Id;
            
            if (inputModel.Users != null)
            {
                foreach (var user in inputModel.Users)
                {
                    model.Targets.Add(new TargetContentItemPermissionViewModel { UserId = user, Checked = true });
                }
            }

            if (inputModel.Customers != null)
            {
                foreach (var user in inputModel.Customers)
                {
                    model.Targets.Add(new TargetContentItemPermissionViewModel { UserId = user, Checked = true });
                }
            }

            if (inputModel.BusinessUnits != null)
            {
                foreach (var item in inputModel.BusinessUnits)
                {
                    model.Targets.Add(new TargetContentItemPermissionViewModel { BusinessUnitId = item, Checked = true });
                }
            }

            if (inputModel.Teams != null)
            {
                foreach (var item in inputModel.Teams)
                {
                    model.Targets.Add(new TargetContentItemPermissionViewModel { TeamId = item, Checked = true });
                }
            }

            // if current user is an operator or customer, then he/she will have Owner/edit access,so he/she must be removed from the list
            // in order to prevent rewriting his privelege on the item
            if (isOperatorOrCustomer)
            {
                var toDelete = model.Targets.Where(c => c.UserId != null && c.UserId == currentUserId);
                foreach (var item in toDelete)
                {
                    model.Targets.Remove(item);
                }
            }

            return model;
        }
    }
}