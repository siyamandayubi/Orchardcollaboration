using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.PermissionProviders
{
    public interface IMasterDetailPermissionProvider : IDependency
    {
        bool HasDetail(IContent content);
        void GrantPermissionToDetail(EditContentPermissionViewModel parameters, IContent content); 
        void DeleteChildrenPermissions(IContent content, ContentItemPermissionDetailRecord permissionRecord);
    }
}