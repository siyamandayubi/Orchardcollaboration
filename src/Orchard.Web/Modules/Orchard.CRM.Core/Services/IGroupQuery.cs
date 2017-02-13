using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public interface IGroupQuery : IDependency
    {
        Collection<KeyValuePair<int?, int>> GetCount(IHqlQuery hglQuery, string entityPath, string groupingProperty);
    }
}