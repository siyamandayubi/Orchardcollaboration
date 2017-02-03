using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector
{
    public class Permissions : IPermissionProvider
    {
        // Operator
        public static readonly Permission ManageSuiteCRMPermission = new Permission { Description = "ManageSuiteCR: Grants the user necessary privileges to import/export data from/to SuiteCRM", Name = "ManageSuiteCRMPermission" };

        public Feature Feature
        {
            get;
            set;
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageSuiteCRMPermission };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageSuiteCRMPermission}
                },
            };
        }
    }
}