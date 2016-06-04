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
using Orchard.Data;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Workflows.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Services
{
    public class CRMContentOwnershipService : ICRMContentOwnershipService
    {
        private static readonly string[] ContentTypesThatOperatorsHasAcceesToUnAssigneds = new[] { "Ticket" };

        protected readonly IOrchardServices orchardServices;
        protected IBasicDataService basicDataService;
        protected IRepository<UserRolesPartRecord> userRolesRepository;
        protected readonly IRepository<RolesPermissionsRecord> rolesPermissionsRepository;
        protected readonly IEnumerable<ICRMContentOwnershipServiceExtension> contentOwnershipServiceExtensions;
        protected readonly IWorkflowManager workflowManager;
        public Localizer T { get; set; }

        public CRMContentOwnershipService(
            IEnumerable<ICRMContentOwnershipServiceExtension> contentOwnershipServiceExtensions,
            IRepository<RolesPermissionsRecord> rolesPermissionsRepository,
            IBasicDataService basicDataService,
            IWorkflowManager workflowManager,
            IOrchardServices orchardServices,
            IRepository<UserRolesPartRecord> userRolesRepository
        )
        {
            this.T = NullLocalizer.Instance;
            this.workflowManager = workflowManager;
            this.basicDataService = basicDataService;
            this.rolesPermissionsRepository = rolesPermissionsRepository;
            this.userRolesRepository = userRolesRepository;
            this.orchardServices = orchardServices;
            this.contentOwnershipServiceExtensions = contentOwnershipServiceExtensions;
        }

        public bool CurrentUserCanChangePermission(IContent contentItem)
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            return this.CurrentUserCanChangePermission(contentItem, modelState);
        }

        public bool CurrentUserCanChangePermission(IContent contentItem, ModelStateDictionary modelState)
        {
            if (this.orchardServices.WorkContext.CurrentUser == null)
            {
                return false;
            }

            var contentPermissionPart = contentItem.As<ContentItemPermissionPart>();
            if (contentPermissionPart == null)
            {
                modelState.AddModelError("id", T("The ContentItem doesn't have any ContentItemPermissionPart part.").ToString());
                return false;
            }

            bool isAdmin = this.orchardServices.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);

            if (isAdmin)
            {
                return true;
            }

            bool isOperator = this.orchardServices.Authorizer.Authorize(Permissions.OperatorPermission);

            var allPermissionRecords = contentPermissionPart.Record.Items;

            if (allPermissionRecords == null)
            {
                allPermissionRecords = new List<ContentItemPermissionDetailRecord>();
            }

            bool isAssignee = allPermissionRecords.Count > 0 && allPermissionRecords.Any(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee);

            // operators can assign unassigned items to some one
            if (!isAssignee && (isOperator || isAdmin) && CRMContentOwnershipService.ContentTypesThatOperatorsHasAcceesToUnAssigneds.Any(c => c == contentItem.ContentItem.ContentType))
            {
                return true;
            }

            int userId = this.orchardServices.WorkContext.CurrentUser.Id;
            var currentUserPermissions = this.GetUserPermissionRecordsForItem(contentItem, userId).ToList();

            return currentUserPermissions.Any(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee);
        }

        public bool CurrentUserCanEditContent(IContent item)
        {
            bool isCustomer = this.IsCurrentUserCustomer();
            bool isOperator = this.orchardServices.Authorizer.Authorize(Permissions.OperatorPermission);
            bool isAdmin = this.orchardServices.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);

            if (isAdmin)
            {
                return true;
            }

            var contentPermissionPart = item.As<ContentItemPermissionPart>();

            if (contentPermissionPart == null)
            {
                return true;
            }

            if (this.orchardServices.WorkContext.CurrentUser == null)
            {
                return false;
            }

            foreach (var extension in this.contentOwnershipServiceExtensions)
            {
                if (extension.CanApply(item, this))
                {
                    if (!extension.HasAccessTo(item, this))
                    {
                        return false;
                    }
                }
            }

            bool isAssignee =
                contentPermissionPart.Record.Items != null &&
                contentPermissionPart.Record.Items.Count > 0 &&
                contentPermissionPart.Record.Items.Any(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee);

            if (!isAssignee && CRMContentOwnershipService.ContentTypesThatOperatorsHasAcceesToUnAssigneds.Any(c => c == item.ContentItem.ContentType))
            {
                if (isOperator || isAdmin)
                {
                    return true;
                }
            }

            int userId = this.orchardServices.WorkContext.CurrentUser.Id;
            var allCurrentUserPermissions = this.GetUserPermissionRecordsForItem(item, userId);

            if (allCurrentUserPermissions.Count(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee ||
                                                     c.AccessType == ContentItemPermissionAccessTypes.SharedForEdit) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CurrentUserIsContentItemAssignee(IContent item)
        {
            if (this.orchardServices.WorkContext.CurrentUser == null)
            {
                return false;
            }

            int userId = this.orchardServices.WorkContext.CurrentUser.Id;
            var allCurrentUserPermissions = this.GetUserPermissionRecordsForItem(item, userId);

            return allCurrentUserPermissions.Count(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee) > 0;
        }

        public bool CurrentUserCanViewContent(IContent item)
        {
            bool isOperator = this.orchardServices.Authorizer.Authorize(Permissions.OperatorPermission);
            bool isAdmin = this.orchardServices.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);

            var contentPermissionPart = item.As<ContentItemPermissionPart>();

            // This authorization is only applied to content which has ContentItemPermissionPart
            if (contentPermissionPart == null)
            {
                return true;
            }

            // none login users arenot allowed to view anything
            if (this.orchardServices.WorkContext.CurrentUser == null)
            {
                return false;
            }

            // admin can do anything
            if (isAdmin)
            {
                return true;
            }

            int userId = this.orchardServices.WorkContext.CurrentUser.Id;

            var allCurrentUserPermissions = this.GetUserPermissionRecordsForItem(item, userId);

            if (contentPermissionPart.Record.Items == null || contentPermissionPart.Record.Items.Count(d => d.AccessType == ContentItemPermissionAccessTypes.Assignee) == 0)
            {
                if ((isAdmin || isOperator) && CRMContentOwnershipService.ContentTypesThatOperatorsHasAcceesToUnAssigneds.Any(c => c == item.ContentItem.ContentType))
                {
                    return true;
                }
            }

            if (allCurrentUserPermissions.Count() > 0 )
            {
                return true;
            }

            return false;
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

        public bool CurrentUserCanDeletePermission(int permissionId, IContent item, ModelStateDictionary modelState)
        {
            ContentItem contentItem = item.ContentItem;
            var contentPermissionPart = contentItem.As<ContentItemPermissionPart>();
            if (contentPermissionPart == null)
            {
                modelState.AddModelError("id", T("The ContentItem doesn't have any ContentItemPermissionPart part.").ToString());
                return false;
            }

            foreach (var extension in this.contentOwnershipServiceExtensions)
            {
                if (extension.CanApply(item, this))
                {
                    if (!extension.HasAccessTo(item, this))
                    {
                        return false;
                    }
                }
            }

            var allPermissionRecords = contentPermissionPart.Record.Items;

            if (allPermissionRecords == null || allPermissionRecords.Count == 0)
            {
                modelState.AddModelError("contentId", T("There is no permission associated with the given contentItem.").ToString());
                return false;
            }

            int userId = this.orchardServices.WorkContext.CurrentUser.Id;
            if (!this.CurrentUserIsContentItemAssignee(contentItem) && !this.orchardServices.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
            {
                modelState.AddModelError("accessType", T("You don't have permission to change access to the contentItem").ToString());
                return false;
            }

            var ownerPermission = allPermissionRecords.FirstOrDefault(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee);
            if (ownerPermission != null && permissionId == ownerPermission.Id)
            {
                modelState.AddModelError("accessType", T("You can't delete owner permission").ToString());
                return false;
            }

            var permissionRecord = allPermissionRecords.FirstOrDefault(c => c.Id == permissionId);
            if (permissionRecord == null)
            {
                throw new System.ArgumentOutOfRangeException("There is no ContentItemPermissionDetailRecord with the Id=" + permissionId.ToString(CultureInfo.InvariantCulture));
            }

            return true;
        }

        public bool IsCurrentUserCustomer()
        {
            return !this.orchardServices.Authorizer.Authorize(Permissions.OperatorPermission) &&
                   !this.orchardServices.Authorizer.Authorize(Permissions.AdvancedOperatorPermission) &&
                   this.orchardServices.Authorizer.Authorize(Permissions.CustomerPermission);
        }

        public bool IsCurrentUserAdvanceOperator()
        {
            return this.orchardServices.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);
        }

        public bool IsCurrentUserOperator()
        {
            return this.orchardServices.Authorizer.Authorize(Permissions.OperatorPermission) ||
                this.orchardServices.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);
        }

        public bool IsCustomer(int userId)
        {
            var roles = this.rolesPermissionsRepository.Table.Where(c =>
                     c.Permission.Name == Permissions.CustomerPermission.Name &&
                     c.Permission.FeatureName == "Orchard.CRM.Core").ToList();

            var customerRoles = roles.ConvertAll(c => c.Role.Id).ToArray();

            return this.userRolesRepository.Table.Any(c => c.UserId == userId && customerRoles.Contains(c.Role.Id));
        }

        public bool IsOperator(int userId)
        {
            var roles = this.rolesPermissionsRepository.Table.Where(c =>
                     (c.Permission.Name == Permissions.OperatorPermission.Name || c.Permission.Name == Permissions.AdvancedOperatorPermission.Name) &&
                     c.Permission.FeatureName == "Orchard.CRM.Core").ToList();

            var operatorRoles = roles.ConvertAll(c => c.Role.Id).ToArray();

            return this.userRolesRepository.Table.Any(c => c.UserId == userId && operatorRoles.Contains(c.Role.Id));
        }
    }
}