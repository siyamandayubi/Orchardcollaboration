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