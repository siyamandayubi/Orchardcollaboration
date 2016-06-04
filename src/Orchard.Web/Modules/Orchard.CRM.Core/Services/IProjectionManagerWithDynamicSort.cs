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