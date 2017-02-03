using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Services
{
    public interface IProjectSearchService : IDependency
    {
        IPageOfItems<IContent> Query(PagerParametersWithSortFields pagerParameters, string searchPhrase, int? projectId, string[] contentTypes, string[] searchFields);
    }
}
 