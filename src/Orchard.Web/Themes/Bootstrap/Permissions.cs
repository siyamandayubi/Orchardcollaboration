using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Bootstrap {
    public class Permissions : IPermissionProvider {

        public static readonly Permission ManageThemeSettings = new Permission { Description = "Manage Theme Settings", Name = "ManageThemeSettings" };
        public static readonly Permission EditCustomCss = new Permission { Description = "Edit Custom CSS", Name = "EditCustomCss" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageThemeSettings,
                EditCustomCss,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageThemeSettings, EditCustomCss }
                },
            };
        }
    }
}