using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core
{
    public class Permissions : IPermissionProvider
    {
        // Operator
        public static readonly Permission OperatorPermission = new Permission { Description = "OperatorPermission: Allows user to create new item or edit/view/assign/comment items he/she has edit/owner access to them", Name = "OperatorPermission" };
        public static readonly Permission CustomerPermission = new Permission { Description = "CustomerPermission: The permission allows uset to create/view/comment his/her own items.", Name = "CustomerPermission" };
        public static readonly Permission BasicDataPermission = new Permission { Description = "BasicDataPermission: The permission to create/edit/deleter basic Data", Name = "BasicDataPermission" };
        public static readonly Permission AdvancedOperatorPermission = new Permission { Description = "AdvancedOperatorPermission: Grant user an unlimited access to the items", Name = "AdvancedOperatorPermission" };

        public Feature Feature
        {
            get;
            set;
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
            OperatorPermission,
            CustomerPermission,
            BasicDataPermission,
            AdvancedOperatorPermission
        };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {AdvancedOperatorPermission, OperatorPermission, BasicDataPermission}
                },

                new PermissionStereotype {
                    Name = "Operator",
                    Permissions = new[] {OperatorPermission, BasicDataPermission}
                },

                new PermissionStereotype{
                    Name = "Customer",
                    Permissions = new []
                    {
                        CustomerPermission,
                        Orchard.Core.Contents.Permissions.EditContent
                    }
                }
            };
        }
    }
}