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