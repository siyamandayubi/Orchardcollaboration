using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.CRM.Core.Services
{
    public interface ISearchTicketService : IDependency
    {
        int CountByIndexProvider(PostedTicketSearchViewModel searchModel);
        IContent[] SearchByIndexProvider(PagerParametersWithSortFields pagerParameters, PostedTicketSearchViewModel searchModel);
        IContent[] SearchByDatabase(PagerParametersWithSortFields pagerParameters, PostedTicketSearchViewModel searchModel);
        int CountByDatabase(PostedTicketSearchViewModel searchModel);
        IHqlQuery CreateQuery(PostedTicketSearchViewModel searchModel);
        string CreateLucenePermissionQuery(PostedTicketSearchViewModel searchModel);

        TicketPart GetByTicketNumber(int number);
    }
}
