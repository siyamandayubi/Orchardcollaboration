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