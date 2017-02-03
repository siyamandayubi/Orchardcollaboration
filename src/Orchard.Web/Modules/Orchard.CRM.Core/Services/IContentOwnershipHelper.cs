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