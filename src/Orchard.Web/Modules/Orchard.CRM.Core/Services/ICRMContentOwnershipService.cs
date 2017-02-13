using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Services
{
    public interface ICRMContentOwnershipService : IDependency
    {
        bool IsOperator(int userId);
        bool IsCustomer(int userId);
        bool CurrentUserCanChangePermission(IContent contentItem);
        bool CurrentUserCanChangePermission(IContent item, ModelStateDictionary modelState);
        bool CurrentUserIsContentItemAssignee(IContent item);
        bool CurrentUserCanEditContent(IContent item);
        bool CurrentUserCanDeletePermission(int permissionId, IContent item, ModelStateDictionary modelState);
        bool CurrentUserCanViewContent(IContent item);
        bool IsCurrentUserCustomer();
        bool IsCurrentUserOperator();
        bool IsCurrentUserAdvanceOperator();
    }
}