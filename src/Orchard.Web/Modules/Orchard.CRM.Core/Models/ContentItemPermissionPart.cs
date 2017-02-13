using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class ContentItemPermissionPart : ContentPart<ContentItemPermissionPartRecord>
    {
        public const string PermissionsSearchFieldName = "Permissions";
        public const string OwnerSearchFieldName = "Owners";
        public const string EmptyPermissionSearchFieldName = "UNASSIGNED";
    }
}