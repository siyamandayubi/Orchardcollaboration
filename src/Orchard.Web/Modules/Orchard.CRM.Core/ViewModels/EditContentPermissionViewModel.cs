using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class EditContentPermissionViewModel
    {
        private Collection<TargetContentItemPermissionViewModel> targets = new Collection<TargetContentItemPermissionViewModel>();

        public int[] ContentIds { get; set; }
        public byte AccessType { get; set; }
        public bool RemoveOldPermission { get; set; }
        public Collection<TargetContentItemPermissionViewModel> Targets
        {
            get
            {
                return this.targets;
            }
        }
    }

    public class PostEditContentPermissionViewModel
    {
        public bool ApplyToChildren { get; set; }
        public int[] ContentIds { get; set; }
        public byte AccessType { get; set; }
        public bool RemoveOldPermission { get; set; }
        public int[] Users { get; set; }
        public int[] Customers { get; set; }
        public int[] BusinessUnits { get; set; }
        public int[] Teams { get; set; }

        public static EditContentPermissionViewModel Convert(PostEditContentPermissionViewModel inputModel)
        {
            EditContentPermissionViewModel model = new EditContentPermissionViewModel
            {
                ContentIds = inputModel.ContentIds,
                AccessType = inputModel.AccessType,
                RemoveOldPermission = inputModel.RemoveOldPermission,
            };

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

            return model;
        }
    }

}