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