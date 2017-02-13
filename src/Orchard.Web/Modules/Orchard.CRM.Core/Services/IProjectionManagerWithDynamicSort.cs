using Orchard.ContentManagement;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public interface IProjectionManagerWithDynamicSort : IProjectionManager
    {
        IHqlQuery ApplyFilter(IHqlQuery contentQuery, string category, string type, dynamic state);
        IHqlQuery AddSortCriterion(string category, string type, dynamic state, IHqlQuery groupQuery);

        /// <summary>
        /// Container will be used by token
        /// </summary>
        /// <param name="container">Container will be used by token</param>
        /// <returns></returns>
        IEnumerable<ContentItem> GetContentItems(int queryId, IContent container, int skip = 0, int count = 0);

        /// <summary>
        /// Container will be used by token
        /// </summary>
        /// <param name="container">Container will be used by token</param>
        /// <returns></returns>
        int GetCount(int queryId, IContent container);

        /// <summary>
        /// Container will be used by token
        /// </summary>
        /// <param name="container">Container will be used by token</param>
        /// <returns></returns>
        IEnumerable<IHqlQuery> GetContentQueries(QueryPartRecord queryRecord, IEnumerable<SortCriterionRecord> sortCriteria, IContent container);
    }
}