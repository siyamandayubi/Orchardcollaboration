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
using Orchard.CRM.Core.ViewModels;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Services
{
    public interface IContentOwnershipHelper : IDependency
    {
        IEnumerable<IUser> GetCustomersWhoHaveAccessToThisContent(IContent content);
        void RestrictToPeopleWhoHavePermissionInGivenItem(ContentItemSetPermissionsViewModel model, IContent content);
        bool IsChangingPermissionsValid(EditContentPermissionViewModel model, IList<ContentItem> contentItems, ModelStateDictionary modelState);
        void Create(ContentItemPermissionDetailRecord permission, ContentItem contentItem, bool triggerActivity);
        ContentItemSetPermissionsViewModel CreateModel();
        IEnumerable<ContentItemPermissionDetailRecord> GetUserPermissionRecordsForItem(IContent item, int userId);
        void FillPermissions(ContentItemSetPermissionsViewModel model, IEnumerable<ContentItem> contentItems);
        void FillPermissions(ContentItemSetPermissionsViewModel model, IEnumerable<ContentItem> contentItems, bool onlyAddOwnerPermissions);
        void Update(EditContentPermissionViewModel model, IList<ContentItem> contentItems);
        void Update(EditContentPermissionViewModel model, IList<ContentItem> contentItems, bool writeToActivityStream);
    }
}