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

using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Project.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;

namespace Orchard.CRM.Project.Handlers
{
    public class MilestoneHandler : ContentHandler
    {
        public MilestoneHandler(
            IRepository<MilestonePartRecord> repository,
            IContentManager contentManager,
            IContentOwnershipHelper contentOwnershipHelper)
        {
            Filters.Add(StorageFilter.For(repository));

            this.OnPublishing<AttachToMilestonePart>((context, part) =>
            {
                if (part.Record.MilestoneId.HasValue)
                {
                    var milestone = contentManager.Get(part.Record.MilestoneId.Value);

                    ContentItemSetPermissionsViewModel milestonePermissions = new ContentItemSetPermissionsViewModel();
                    contentOwnershipHelper.FillPermissions(milestonePermissions, new[] { milestone });

                    ContentItemSetPermissionsViewModel itemPermissions = new ContentItemSetPermissionsViewModel();
                    contentOwnershipHelper.FillPermissions(itemPermissions, new[] { part.ContentItem });

                    EditContentPermissionViewModel editContentPermissionViewModel = new EditContentPermissionViewModel();
                    editContentPermissionViewModel.AccessType = ContentItemPermissionAccessTypes.SharedForView;
                    editContentPermissionViewModel.RemoveOldPermission = false;

                    // Grant users who don't have access to item
                    foreach(var user in milestonePermissions.Users.Where(c=> !itemPermissions.Users.Any(d=>d.Value ==c.Value)))
                    {
                        editContentPermissionViewModel.Targets.Add(new TargetContentItemPermissionViewModel { UserId = int.Parse(user.Value), Checked = true });
                    }

                    // Grant customer who don't have access to item
                    foreach (var user in milestonePermissions.Customers.Where(c => !itemPermissions.Customers.Any(d => d.Value == c.Value)))
                    {
                        editContentPermissionViewModel.Targets.Add(new TargetContentItemPermissionViewModel { UserId = int.Parse(user.Value), Checked = true });
                    }

                    // Grant businessUnits who don't have access to item
                    foreach (var bussinesUnit in milestonePermissions.BusinessUnits.Where(c => !itemPermissions.BusinessUnits.Any(d => d.BusinessUnitId == c.BusinessUnitId)))
                    {
                        editContentPermissionViewModel.Targets.Add(new TargetContentItemPermissionViewModel { BusinessUnitId = bussinesUnit.BusinessUnitId, Checked = true });
                    }

                    contentOwnershipHelper.Update(editContentPermissionViewModel, new[] { part.ContentItem }, false);
                }
            });
        }
    }
}