using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.ViewModels
{
    public class ContentItemSetPermissionsViewModel
    {
        private Collection<TeamViewModel> teams = new Collection<TeamViewModel>();
        private Collection<BusinessUnitViewModel> businessUnits = new Collection<BusinessUnitViewModel>();
        private Collection<SelectListItem> users = new Collection<SelectListItem>();
        private Collection<SelectListItem> customers = new Collection<SelectListItem>();
        private Collection<ContentItemPermissionsModel> contentItems = new Collection<ContentItemPermissionsModel>();

        public Collection<ContentItemPermissionsModel> ContentItems
        {
            get
            {
                return this.contentItems;
            }
        }

        public int Id { get; set; }

        public string ReturnUrl { get; set; }

        /// <summary>
        /// Used for custom values
        /// </summary>
        public object Tag { get; set; }

        public bool IsCurrentUserAdmin { get; set; }
        public bool IsCurrentUserCustomer { get; set; }
        public bool HasChildren { get; set; }
        public Collection<TeamViewModel> Teams
        {
            get
            {
                return this.teams;
            }
        }

        public Collection<BusinessUnitViewModel> BusinessUnits
        {
            get
            {
                return this.businessUnits;
            }
        }

        public Collection<SelectListItem> Users
        {
            get
            {
                return this.users;
            }
        }

        public Collection<SelectListItem> Customers
        {
            get
            {
                return this.customers;
            }
        }
        
        public class ContentItemPermissionsModel
        {
            private Collection<ItemPermissionViewModel> currentPermissions = new Collection<ItemPermissionViewModel>();

            public ContentItem ContentItem { get; set; }
            public dynamic ContentItemShape { get; set; }

            public bool IsCurrentUserOwner { get; set; }
            public bool CurrentUserHasRightToChangePermissions { get; set; }

            public Collection<ItemPermissionViewModel> CurrentPermissions
            {
                get
                {
                    return this.currentPermissions;
                }
            }
        }

        public class ItemPermissionViewModel : TargetContentItemPermissionViewModel
        {
            public int Id { get; set; }
            public int ContentItemId { get; set; }
            public byte AccessType { get; set; }
            public string Name { get; set; }
        }
    }
}