using Orchard.Security.Permissions;
using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public class PermissionProvider : IRuleProvider
    {
        private IEnumerable<IPermissionProvider> permissionProviders;
        private List<Permission> permissions = new List<Permission>();

        public IOrchardServices Services { get; private set; }

        public PermissionProvider(IOrchardServices services, IEnumerable<IPermissionProvider> permissionProviders)
        {
            this.permissionProviders = permissionProviders;
            this.Services = services;

            foreach (var provider in this.permissionProviders)
            {
                permissions.AddRange(provider.GetPermissions());
            }
        }

        public void Process(RuleContext ruleContext)
        {
            if (!String.Equals(ruleContext.FunctionName, "permission", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var permissionName = Convert.ToString(ruleContext.Arguments[0]);

            Permission permission = this.permissions.FirstOrDefault(c=>c.Name == permissionName);

            if (permission == null)
            {
                ruleContext.Result = false;
            }

            ruleContext.Result = this.Services.Authorizer.Authorize(permission);
        }
    }
}