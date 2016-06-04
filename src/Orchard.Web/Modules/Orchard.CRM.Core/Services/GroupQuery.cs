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

using NHibernate;
using NHibernate.Transform;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public class GroupQuery : IGroupQuery
    {
        private readonly Lazy<ISessionLocator> sessionLocator;

        public GroupQuery(Lazy<ISessionLocator> sessionLocator)
        {
            this.sessionLocator = sessionLocator;
        }

        public Collection<KeyValuePair<int?, int>> GetCount(IHqlQuery hglQuery, string entityPath, string groupingProperty)
        {
            DefaultHqlQuery defaultQuery = hglQuery as DefaultHqlQuery;
            var hql = defaultQuery.ToHql(true);
            hql = string.Format("select count(*), k.{0} from Orchard.ContentManagement.Records.ContentItemVersionRecord as c join c.ContentItemRecord as d join d.{1} as k where c.Id in ({2}) and (c.Published = True) group by k.{0}", groupingProperty, entityPath, hql);
            var session = this.sessionLocator.Value.For(typeof(ContentItem));

            var query = session
                .CreateQuery(hql)
                .SetCacheable(false)
                .SetResultTransformer(Transformers.AliasToEntityMap)
                .List<IDictionary>();

            Collection<KeyValuePair<int?, int>> result = new Collection<KeyValuePair<int?, int>>();
            foreach (var group in query)
            {
                int value = int.Parse(group["0"].ToString());
                int? key = group["1"] != null ? (int?)int.Parse(group["1"].ToString()) : null;
                result.Add(new KeyValuePair<int?, int>(key, value));
            }

            return result;
        }

        private class Query : DefaultHqlQuery
        {
            public Query(IContentManager contentManager,
            ISession session,
            IEnumerable<ISqlStatementProvider> sqlStatementProviders,
            ShellSettings shellSettings)
                : base(contentManager, session, sqlStatementProviders, shellSettings)
            {
            }
        }
    }
}