using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.PermissionProviders
{
    public class MasterDetailPermissionManager : IMasterDetailPermissionManager
    {
        private readonly IEnumerable<IMasterDetailPermissionProvider> providers;
        
        public MasterDetailPermissionManager(IEnumerable<IMasterDetailPermissionProvider> providers)
        {
            this.providers = providers;
        }

        public bool HasChildItems(IContent content)
        {
            return providers.Any(c => c.HasDetail(content));
        }

        public void DeleteChildrenPermissions(IContent content, ContentItemPermissionDetailRecord permissionRecord)
        {
            foreach (var provider in this.providers)
            {
                if (provider.HasDetail(content))
                {
                    provider.DeleteChildrenPermissions(content, permissionRecord);
                }
            }
        }

        public void GrantPermissionToChildren(EditContentPermissionViewModel parameters, IContent content)
        {
            EditContentPermissionViewModel temp = new EditContentPermissionViewModel
            {
                AccessType = parameters.AccessType,
                ContentIds = parameters.ContentIds,
                RemoveOldPermission = parameters.RemoveOldPermission,
            };

            temp.Targets.AddRange(parameters.Targets);

            foreach (var provider in this.providers)
            {
                if (provider.HasDetail(content))
                {
                    provider.GrantPermissionToDetail(temp, content);
                }
            }
        }
    }
}