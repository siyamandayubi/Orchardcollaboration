using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers
{
    public interface ICurrentContentItemForWidgetTokenProvider : IDependency
    {
        IEnumerable<ContentItemPermissionPart> GetCurrentItemsPermissionParts();
    }
}