using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.ActivityStream;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Workflows.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Services
{
    public class ContentOwnershipHelper : IContentOwnershipHelper
    {
        protected readonly IOrchardServices orchardServices;
        protected readonly IBasicDataService basicDataService;
        protected readonly IRepository<UserRolesPartRecord> userRolesRepository;
        protected readonly IRepository<UserPartRecord> userRepository;
        protected readonly IRepository<ContentItemPermissionDetailRecord> permissionDetailRecordRepository;
        protected readonly ICRMContentOwnershipService crmContentOwnershipService;
        protected readonly IWorkflowManager workflowManager;
        protected readonly IActivityStreamService activityStreamService;
        protected readonly IRepository<RolesPermissionsRecord> rolesPermissionsRepository;
        public Localizer T { get; set; }

        public ContentOwnershipHelper(
            IRepository<RolesPermissionsRecord> rolesPermissionsRepository,
            IWorkflowManager workflowManager,
            IOrchardServices orchardServices,
            IBasicDataService basicDataService,
            IRepository<UserPartRecord> userRepository,
            ICRMContentOwnershipService crmContentOwnershipService,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IRepository<ContentItemPermissionDetailRecord> permissionDetailRecordRepository,
            IActivityStreamService activityStreamService
        )
        {
            this.T = NullLocalizer.Instance;
            this.rolesPermissionsRepository = rolesPermissionsRepository;
            this.workflowManager = workflowManager;
            this.permissionDetailRecordRepository = permissionDetailRecordRepository;
            this.userRolesRepository = userRolesRepository;
            this.basicDataService = basicDataService;
            this.orchardServices = orchardServices;
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.userRepository = userRepository;
            this.activityStreamService = activityStreamService;
        }
 
        public bool IsChangingPermissionsValid(EditContentPermissionViewModel model, IList<ContentItem> contentItems, ModelStateDictionary modelState)
        {
            if (model.AccessType == default(byte))
            {
                modelState.AddModelError("AccessType", T("AccessType value is not valid").Text);
                return false;
            }

            foreach (var contentItem in contentItems)
            {
                AttachToProjectPart attachToProjectPart = contentItem.As<AttachToProjectPart>();
                if (attachToProjectPart != null && attachToProjectPart.Record.Project != null)
                {
                    var project = this.orchardServices.ContentManager.Get(attachToProjectPart.Record.Project.Id);
                    foreach (var target in model.Targets)
                    {
                        if (!this.HasAccessToTheContent(project, target))
                        {
                            modelState.AddModelError("Targets", T("The given businessUnit or user doesn't have access to the project").Text);
                            return false;
                        }
                    }
                }

                bool currentUserCanChangePermission = this.crmContentOwnershipService.CurrentUserCanChangePermission(contentItem, modelState);

                if (!currentUserCanChangePermission)
                {
                    return false;
                }

                if (model.Targets.Count(c => c.Checked) > 1 && model.AccessType == ContentItemPermissionAccessTypes.Assignee)
                {
                    modelState.AddModelError("Targets", T("Items atleast must have only one assignee").ToString());
                    return false;
                }

                foreach (var item in model.Targets.Where(c => c.Checked))
                {
                    int?[] temp = new int?[] { item.BusinessUnitId, item.TeamId, item.UserId };
                    if (temp.Count(c => c.HasValue) != 1)
                    {
                        modelState.AddModelError("targetUserId", T("One of the parameters must have a value").ToString());
                        return false;
                    }
                }
            }

            return true;
        }

        public void Create(ContentItemPermissionDetailRecord item, ContentItem contentItem, bool triggerActivity)
        {
            var contentPermissionPart = contentItem.As<ContentItemPermissionPart>();
            if (contentPermissionPart.Record.Items == null)
            {
                contentPermissionPart.Record.Items = new List<ContentItemPermissionDetailRecord>();
            }

            if (!contentPermissionPart.Record.Items.Contains(item))
            {
                contentPermissionPart.Record.Items.Add(item);
            }

            this.permissionDetailRecordRepository.Create(item);
            this.permissionDetailRecordRepository.Flush();

            if (triggerActivity)
            {
                Dictionary<string, object> tokensContext = new Dictionary<string, object>();
                tokensContext.Add("Permission", item);
                this.workflowManager.TriggerEvent("PermissionCreated", contentItem, () => { return tokensContext; });
            }
        }

        public IEnumerable<IUser> GetCustomersWhoHaveAccessToThisContent(IContent content)
        {
            if (content == null) { return null; }

            var contentPermissionPart = content.As<ContentItemPermissionPart>();
            if (contentPermissionPart == null) { return new List<IUser>(); }

            var items = contentPermissionPart.Record.Items;
            if (items == null || items.Count == 0) { return new List<IUser>(); }

            var userIds = items.Where(c => c.User != null).Select(c => c.User.Id).ToArray();

            var roles = this.rolesPermissionsRepository.Table.Where(c =>
                 c.Permission.Name == Permissions.CustomerPermission.Name &&
                 c.Permission.FeatureName == "Orchard.CRM.Core").ToList();

            var customerRoles = roles.ConvertAll(c => c.Role.Id).ToArray();
            var customerUsers = this.userRolesRepository.Table.Where(c => customerRoles.Contains(c.Role.Id) && userIds.Contains(c.UserId)).Select(c => c.UserId).ToList();

            return this.orchardServices.ContentManager.GetMany<IUser>(customerUsers, VersionOptions.Published, new QueryHints());
        }

        public IEnumerable<ContentItemPermissionDetailRecord> GetUserPermissionRecordsForItem(IContent item, int userId)
        {
            var contentPermissionPart = item.As<ContentItemPermissionPart>();

            if (contentPermissionPart == null)
            {
                return new List<ContentItemPermissionDetailRecord>();
            }

            List<int> teams = this.basicDataService.GetTeamMembers()
                .Where(c => c.UserPartRecord.Id == userId)
                .Select(c => c.TeamPartRecord.Id).ToList();

            List<int> businessUnits = this.basicDataService.GetBusinessUnitMembers()
                .Where(c => c.UserPartRecord.Id == userId)
                .Select(c => c.BusinessUnitPartRecord.Id).ToList();

            var allPermissionRecords = contentPermissionPart.Record.Items;

            if (allPermissionRecords == null)
            {
                return new List<ContentItemPermissionDetailRecord>();
            }

            // usr can edit new them
            if (allPermissionRecords.Count == 0)
            {
                return allPermissionRecords;
            }

            var userPermissionRecords = allPermissionRecords.Where(c => c.User != null && c.User.Id == userId).ToList();
            var teamPermissionRecords = allPermissionRecords.Where(c => c.Team != null && teams.Contains(c.Team.Id)).ToList();
            var businessUnitPermissionRecords = allPermissionRecords.Where(c => c.BusinessUnit != null && businessUnits.Contains(c.BusinessUnit.Id)).ToList();

            var allCurrentUserPermissions = userPermissionRecords.Union(teamPermissionRecords).Union(businessUnitPermissionRecords);

            return allCurrentUserPermissions;
        }

        public ContentItemSetPermissionsViewModel CreateModel()
        {
            var teams = this.basicDataService.GetTeams().ToList();
            var businessUnits = this.basicDataService.GetBusinessUnits().ToList();

            var model = new ContentItemSetPermissionsViewModel();

            // fill businessUnits
            foreach (var businessUnit in businessUnits)
            {
                var businessUnitPart = businessUnit.As<BusinessUnitPart>();
                var businessUnitModel = new BusinessUnitViewModel
                {
                    BusinessUnitId = businessUnitPart.Id,
                    Name = businessUnitPart.Name,
                    Description = businessUnitPart.Description,
                };

                foreach (var team in teams)
                {
                    var teamPart = team.As<TeamPart>();

                    if (teamPart.BusinessUnit != null && teamPart.BusinessUnit.Id == businessUnitPart.Id)
                    {
                        var teamViewModel = new TeamViewModel
                        {
                            TeamId = teamPart.Id,
                            BusinessUnitId = teamPart.BusinessUnit != null ? (int?)teamPart.BusinessUnit.Id : null,
                            Name = teamPart.Name,
                            Description = teamPart.Description
                        };

                        model.Teams.Add(teamViewModel);
                        businessUnitModel.Teams.Add(teamViewModel);
                    }
                }

                model.BusinessUnits.Add(businessUnitModel);
            }

            // get users with operator permission
            var users = this.basicDataService.GetOperators().ToList();

            foreach (var user in users)
            {
                model.Users.Add(new SelectListItem
                {
                    Value = user.Id.ToString(CultureInfo.InvariantCulture),
                    Text = CRMHelper.GetFullNameOfUser(user.As<UserPart>())
                });
            }

            return model;
        }

        public void RestrictToPeopleWhoHavePermissionInGivenItem(ContentItemSetPermissionsViewModel model, IContent content)
        {
            if (content == null) { return; }

            var contentPermissionPart = content.As<ContentItemPermissionPart>();
            if (contentPermissionPart == null) { return; }

            var items = contentPermissionPart.Record.Items;
            if (items == null || items.Count == 0) { return; }

            var users = items.Where(c => c.User != null).Select(c => c.User).ToList();
            var businessUnits = items.Where(c => c.BusinessUnit != null).Select(c => c.BusinessUnit).ToList();

            var toRemoveUsers = model.Users.Where(c => !users.Any(d => d.Id.ToString(CultureInfo.InvariantCulture) == c.Value)).ToList();
            var toRemoveCustomers = model.Customers.Where(c => !users.Any(d => d.Id.ToString(CultureInfo.InvariantCulture) == c.Value)).ToList();
            var toRemoveBusinessUnits = model.BusinessUnits.Where(c => !businessUnits.Any(d => d.Id == c.BusinessUnitId)).ToList();

            foreach (var toRemove in toRemoveUsers)
            {
                model.Users.Remove(toRemove);
            }

            foreach (var toRemoveCustomer in toRemoveCustomers)
            {
                model.Customers.Remove(toRemoveCustomer);
            }
            
            foreach (var toRemove in toRemoveBusinessUnits)
            {
                model.BusinessUnits.Remove(toRemove);
            }
        }

        public void FillPermissions(ContentItemSetPermissionsViewModel model, IEnumerable<ContentItem> contentItems)
        {
            this.FillPermissions(model, contentItems, false);
        }

        public void FillPermissions(ContentItemSetPermissionsViewModel model, IEnumerable<ContentItem> contentItems, bool onlyAddOwnerPermissions)
        {
            var businessUnits = this.basicDataService.GetBusinessUnits().ToList();
            var teams = this.basicDataService.GetTeams().ToList();
            if (contentItems != null && contentItems.Count() > 0)
            {
                foreach (var contentItem in contentItems)
                {
                    ContentItemSetPermissionsViewModel.ContentItemPermissionsModel contentItemPermissionModel = new ContentItemSetPermissionsViewModel.ContentItemPermissionsModel();
                    contentItemPermissionModel.ContentItem = contentItem;
                    model.ContentItems.Add(contentItemPermissionModel);

                    var contentPermissionPart = contentItem.As<ContentItemPermissionPart>();
                    if (contentPermissionPart == null)
                    {
                        throw new Security.OrchardSecurityException(T("The ContentItem doesn't have any ContentItemPermissionPart part."));
                    }

                    var allPermissionRecords = contentPermissionPart.Record.Items;

                    if (allPermissionRecords == null)
                    {
                        allPermissionRecords = new List<ContentItemPermissionDetailRecord>();
                    }

                    contentItemPermissionModel.IsCurrentUserOwner = this.crmContentOwnershipService.CurrentUserIsContentItemAssignee(contentItem);

                    // operator permission can access unassigned items
                    contentItemPermissionModel.CurrentUserHasRightToChangePermissions = this.crmContentOwnershipService.CurrentUserCanChangePermission(contentItem, new ModelStateDictionary());

                    if (onlyAddOwnerPermissions)
                    {
                        allPermissionRecords = allPermissionRecords.Where(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee).ToList();
                    }

                    // fill the list of current permission items
                    foreach (var itemPermission in allPermissionRecords)
                    {
                        var itemModel = new ContentItemSetPermissionsViewModel.ItemPermissionViewModel
                        {
                            Id = itemPermission.Id,
                            ContentItemId = contentItem.Id,
                            AccessType = itemPermission.AccessType,
                            BusinessUnitId = itemPermission.BusinessUnit != null ? (int?)itemPermission.BusinessUnit.Id : null,
                            TeamId = itemPermission.Team != null ? (int?)itemPermission.Team.Id : null,
                            UserId = itemPermission.User != null ? (int?)itemPermission.User.Id : null,
                        };

                        if (itemPermission.User != null)
                        {
                            var user = this.basicDataService.GetOperatorOrCustomerUser(itemPermission.User.Id);
                            itemModel.Name = CRMHelper.GetFullNameOfUser(user);
                        }
                        else if (itemPermission.Team != null)
                        {
                            var team = teams.FirstOrDefault(c => c.Id == itemPermission.Team.Id);
                            itemModel.Name = team != null ? team.As<TeamPart>().Name : string.Empty;
                        }
                        else if (itemPermission.BusinessUnit != null)
                        {
                            var businessUnit = businessUnits.FirstOrDefault(c => c.Id == itemPermission.BusinessUnit.Id);
                            if (businessUnit != null && businessUnit.As<BusinessUnitPart>() != null)
                            {
                                itemModel.Name = businessUnit.As<BusinessUnitPart>().Name;
                            }
                        }

                        // checked the business Unit in the list
                        if (itemModel.BusinessUnitId.HasValue)
                        {
                            var businessUnit = model.BusinessUnits.FirstOrDefault(c => c.BusinessUnitId == itemModel.BusinessUnitId.Value);
                            if (businessUnit != null)
                            {
                                businessUnit.Checked = true;
                            }
                        }

                        // checked the team in the list
                        if (itemModel.TeamId.HasValue)
                        {
                            var team = model.Teams.FirstOrDefault(c => c.TeamId == itemModel.TeamId.Value);
                            if (team != null)
                            {
                                team.Checked = true;
                            }
                        }

                        // checked the user in the list
                        if (itemModel.UserId.HasValue)
                        {
                            var user = model.Users.FirstOrDefault(c => c.Value == itemModel.UserId.Value.ToString(CultureInfo.InvariantCulture));
                            if (user != null)
                            {
                                user.Selected = true;
                            }
                        }

                        contentItemPermissionModel.CurrentPermissions.Add(itemModel);
                    }
                }
            }
        }

        public void Update(EditContentPermissionViewModel model, IList<ContentItem> contentItems)
        {
            this.Update(model, contentItems, true);
        }

        public void Update(EditContentPermissionViewModel model, IList<ContentItem> contentItems, bool writeToActivityStream)
        {
            foreach (var contentItem in contentItems)
            {
                var snapshot = this.activityStreamService.TakeSnapshot(contentItem);

                var contentPermissionPart = contentItem.As<ContentItemPermissionPart>();

                if (contentPermissionPart.Record.Items == null)
                {
                    contentPermissionPart.Record.Items = new List<ContentItemPermissionDetailRecord>();
                }

                var allPermissionRecords = contentPermissionPart.Record.Items;

                bool isAdmin = this.orchardServices.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);

                int userId = this.orchardServices.WorkContext.CurrentUser.Id;
                var currentUserPermissions = isAdmin ? allPermissionRecords.ToList() : this.GetUserPermissionRecordsForItem(contentItem, userId).ToList();

                var targets = model.Targets.Where(c => c.Checked).ToList();

                bool hasAssignee = allPermissionRecords.Any(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee);

                // if item has assignee and there are some permissions for the item, but none of them belongs to the user, then the user doesn't have access to the item
                if (allPermissionRecords.Count > 0 && hasAssignee)
                {
                    if (!isAdmin && currentUserPermissions.Count(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee ||
                                                         c.AccessType == ContentItemPermissionAccessTypes.SharedForEdit) == 0)
                    {
                        throw new Security.OrchardSecurityException(T("You don't have permission to change access to the contentItem"));
                    }

                    // only assignee can change the assignee
                    if (!isAdmin && currentUserPermissions.Count(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee) == 0 &&
                        model.AccessType == ContentItemPermissionAccessTypes.Assignee)
                    {
                        throw new Security.OrchardSecurityException(T("You don't have permission to change access to the contentItem"));
                    }
                }
                else
                {
                    // if there is no permission for current user, then he/she must have a EditShare permission
                    if (model.Targets.Count(c => c.UserId == userId) == 0 &&
                        !allPermissionRecords.Any(c => c.User != null && c.User.Id == userId))
                    {
                        ContentItemPermissionDetailRecord newPermission = new ContentItemPermissionDetailRecord
                        {
                            AccessType = ContentItemPermissionAccessTypes.SharedForEdit,
                            ContentItemPermissionPartRecord = contentPermissionPart.Record,
                            User = new Users.Models.UserPartRecord { Id = userId }
                        };

                        contentPermissionPart.Record.Items.Add(newPermission);
                        this.Create(newPermission, contentItem, false);
                    }
                }

                foreach (var item in targets)
                {
                    int?[] temp = new int?[] { item.BusinessUnitId, item.TeamId, item.UserId };
                    if (temp.Count(c => c.HasValue) != 1)
                    {
                        throw new Security.OrchardSecurityException(T("You don't have permission to change access to the contentItem"));
                    }

                    // try to find a permission with the same user, team and business values, if there is such permission, we
                    // must not create a new permission and we just have to change the AccessType of the exsiting one
                    var existingPermission = allPermissionRecords.FirstOrDefault(d =>
                        ((item.TeamId == null && d.Team == null) || (item.TeamId.HasValue && d.Team != null && item.TeamId.Value == d.Team.Id)) &&
                        ((item.BusinessUnitId == null && d.BusinessUnit == null) || (item.BusinessUnitId.HasValue && d.BusinessUnit != null && item.BusinessUnitId.Value == d.BusinessUnit.Id)) &&
                        ((item.UserId == null && d.User == null) || (item.UserId.HasValue && d.User != null && item.UserId.Value == d.User.Id))
                        );

                    if (existingPermission != null)
                    {
                        existingPermission.AccessType = model.AccessType;

                        // we should remove the permission from currentUserPermissions in order to prevent it to be deleted
                        currentUserPermissions.Remove(existingPermission);

                        continue;
                    }

                    // setting the parent doesn't have been done.
                    ContentItemPermissionDetailRecord newPermission = new ContentItemPermissionDetailRecord { AccessType = model.AccessType, ContentItemPermissionPartRecord = contentPermissionPart.Record };

                    if (item.BusinessUnitId.HasValue)
                    {
                        var businessUnitRecord = this.basicDataService.GetBusinessUnits().Select(c => c.As<BusinessUnitPart>()).FirstOrDefault(c => c.Id == item.BusinessUnitId.Value);
                        if (businessUnitRecord != null)
                        {
                            newPermission.BusinessUnit = new BusinessUnitPartRecord { Id = item.BusinessUnitId.Value, Name = businessUnitRecord.Name };
                        }
                    }

                    if (item.TeamId.HasValue)
                    {
                        var teamRecord = this.basicDataService.GetTeams().Select(c => c.As<TeamPart>()).FirstOrDefault(c => c.Id == item.TeamId.Value);
                        if (teamRecord != null)
                        {
                            newPermission.Team = new TeamPartRecord { Id = item.TeamId.Value, Name = teamRecord.Name };
                        }
                    }

                    if (item.UserId.HasValue)
                    {
                        var userRecord = this.userRepository.Table.FirstOrDefault(c => c.Id == item.UserId.Value);
                        if (userRecord != null)
                        {
                            newPermission.User = new Users.Models.UserPartRecord { Id = item.UserId.Value, UserName = userRecord.UserName, Email = userRecord.Email };
                        }
                    }

                    contentPermissionPart.Record.Items.Add(newPermission);
                    this.Create(newPermission, contentItem, true);
                }

                if (model.AccessType == ContentItemPermissionAccessTypes.Assignee)
                {
                    if (model.RemoveOldPermission)
                    {
                        // remove previous assignee and sharedForEdit permissions
                        foreach (var permission in currentUserPermissions.Where(c => c.AccessType != ContentItemPermissionAccessTypes.SharedForView))
                        {
                            contentPermissionPart.Record.Items.Remove(permission);
                            this.permissionDetailRecordRepository.Delete(permission);
                        }
                    }
                    else
                    {
                        foreach (var permission in currentUserPermissions.Where(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee))
                        {
                            permission.AccessType = ContentItemPermissionAccessTypes.SharedForEdit;
                        }
                    }
                }

                contentPermissionPart.Record.HasOwner = contentPermissionPart
                        .Record
                        .Items
                        .Count(d =>
                            d.AccessType == ContentItemPermissionAccessTypes.Assignee &&
                            (d.User != null || d.Team != null || d.BusinessUnit != null)) > 0;

                this.permissionDetailRecordRepository.Flush();

                if (writeToActivityStream)
                {
                    this.activityStreamService.WriteChangesToStreamActivity(contentItem, snapshot, StreamWriters.ContentItemPermissionStreamWriter);
                }
            }
        }
     
        private bool HasAccessToTheContent(IContent content, TargetContentItemPermissionViewModel target)
        {
            if (content == null) { return false; }

            var contentPermissionPart = content.As<ContentItemPermissionPart>();
            if (contentPermissionPart == null) { return false; }

            var items = contentPermissionPart.Record.Items;
            if (items == null || items.Count == 0) { return false; }

            if (target.UserId.HasValue)
            {
                return items.Any(c => c.User != null && c.User.Id == target.UserId.Value);
            }
            else if (target.BusinessUnitId.HasValue)
            {
                return items.Any(c => c.BusinessUnit != null && c.BusinessUnit.Id == target.BusinessUnitId.Value);
            }
            else if (target.TeamId.HasValue)
            {
                return items.Any(c => c.Team != null && c.Team.Id == target.TeamId.Value);
            }

            return true;
        }
    }
}