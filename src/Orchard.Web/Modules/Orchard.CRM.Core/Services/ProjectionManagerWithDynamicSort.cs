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
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.DisplayManagement.Shapes;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Orchard.CRM.Core.Services
{
    public class ProjectionManagerWithDynamicSort : IProjectionManagerWithDynamicSort
    {
        private readonly RequestContext requestContext;
        private readonly ITokenizer _tokenizer;
        private readonly IEnumerable<IFilterProvider> _filterProviders;
        private readonly IEnumerable<ISortCriterionProvider> _sortCriterionProviders;
        private readonly IEnumerable<ILayoutProvider> _layoutProviders;
        private readonly IEnumerable<IPropertyProvider> _propertyProviders;
        private readonly IContentManager _contentManager;
        private readonly IRepository<QueryPartRecord> _queryRepository;
        private IEnumerable<TypeDescriptor<SortCriterionDescriptor>> availableSortCriteria;

        public ProjectionManagerWithDynamicSort(
            ITokenizer tokenizer,
            IEnumerable<IFilterProvider> filterProviders,
            IEnumerable<ISortCriterionProvider> sortCriterionProviders,
            IEnumerable<ILayoutProvider> layoutProviders,
            IEnumerable<IPropertyProvider> propertyProviders,
            IContentManager contentManager,
            RequestContext requestContext,
            IRepository<QueryPartRecord> queryRepository)
        {

            this.requestContext = requestContext;
            _tokenizer = tokenizer;
            _filterProviders = filterProviders;
            _sortCriterionProviders = sortCriterionProviders;
            _layoutProviders = layoutProviders;
            _propertyProviders = propertyProviders;
            _contentManager = contentManager;
            _queryRepository = queryRepository;
            T = NullLocalizer.Instance;
        }

        protected IEnumerable<TypeDescriptor<FilterDescriptor>> availableFilters = null;

        public Localizer T { get; set; }

        public static DynamicProjectionSortModel GetSortModelFromRequest(RequestContext requestContext)
        {
            if (requestContext.HttpContext == null)
            {
                return null;
            }

            var httpRequest = requestContext.HttpContext.Request;
            NameValueCollection requestCollection = httpRequest.HttpMethod == "POST" ? httpRequest.Form : httpRequest.QueryString;
            string prefix = "SortModel.";
            if (requestCollection.AllKeys.Count(c => c.StartsWith(prefix)) > 0)
            {
                string categoryName = prefix + "Category";
                string directionName = prefix + "Direction";
                string typeName = prefix + "Type";

                if (requestCollection.AllKeys.Contains(categoryName) &&
                    requestCollection.AllKeys.Contains(typeName) &&
                    requestCollection.AllKeys.Contains(categoryName))
                {
                    return new DynamicProjectionSortModel
                    {
                        Type = requestCollection[typeName],
                        Category = requestCollection[categoryName],
                        Direction = bool.Parse(requestCollection[directionName])
                    };
                }
            }

            return null;
        }

        public IEnumerable<TypeDescriptor<FilterDescriptor>> DescribeFilters()
        {
            var context = new DescribeFilterContext();

            foreach (var provider in _filterProviders)
            {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<SortCriterionDescriptor>> DescribeSortCriteria()
        {
            var context = new DescribeSortCriterionContext();

            foreach (var provider in _sortCriterionProviders)
            {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<LayoutDescriptor>> DescribeLayouts()
        {
            var context = new DescribeLayoutContext();

            foreach (var provider in _layoutProviders)
            {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<PropertyDescriptor>> DescribeProperties()
        {
            var context = new DescribePropertyContext();

            foreach (var provider in _propertyProviders)
            {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public FilterDescriptor GetFilter(string category, string type)
        {
            return DescribeFilters()
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(x => x.Category == category && x.Type == type);
        }

        public SortCriterionDescriptor GetSortCriterion(string category, string type)
        {
            return DescribeSortCriteria()
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(x => x.Category == category && x.Type == type);
        }

        public LayoutDescriptor GetLayout(string category, string type)
        {
            return DescribeLayouts()
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(x => x.Category == category && x.Type == type);
        }

        public PropertyDescriptor GetProperty(string category, string type)
        {
            return DescribeProperties()
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(x => x.Category == category && x.Type == type);
        }

        public int GetCount(int queryId)
        {
            return this.GetCount(queryId, null);
        }

        public int GetCount(int queryId, IContent container)
        {

            var queryRecord = _queryRepository.Get(queryId);

            if (queryRecord == null)
            {
                throw new ArgumentException("queryId");
            }

            // aggregate the result for each group query

            return GetContentQueries(queryRecord, Enumerable.Empty<SortCriterionRecord>(), container)
                .Sum(contentQuery => contentQuery.Count());
        }

        public IEnumerable<ContentItem> GetContentItems(int queryId, int skip = 0, int count = 0)
        {
            return this.GetContentItems(queryId, null, skip, count);
        }

        public IEnumerable<ContentItem> GetContentItems(int queryId, IContent container, int skip = 0, int count = 0)
        {
            this.availableSortCriteria = this.DescribeSortCriteria().ToList();

            var queryRecord = _queryRepository.Get(queryId);

            if (queryRecord == null)
            {
                throw new ArgumentException("queryId");
            }

            var contentItems = new List<ContentItem>();

            // aggregate the result for each group query
            foreach (var contentQuery in GetContentQueries(queryRecord, queryRecord.SortCriteria, container))
            {
                contentItems.AddRange(contentQuery.Slice(skip, count));
            }

            if (queryRecord.FilterGroups.Count <= 1)
            {
                return contentItems;
            }

            // re-executing the sorting with the cumulated groups
            var ids = contentItems.Select(c => c.Id).ToArray();

            if (ids.Length == 0)
            {
                return Enumerable.Empty<ContentItem>();
            }

            var groupQuery = _contentManager.HqlQuery().Where(alias => alias.Named("ci"), x => x.InG("Id", ids));

            // iterate over each sort criteria to apply the alterations to the query object
            var sortModel = GetSortModelFromRequest(this.requestContext);

            if (sortModel != null)
            {
                dynamic state = new Composite();
                state.Sort = sortModel.Direction;
                groupQuery = this.AddSortCriterion(sortModel.Category, sortModel.Type, state, groupQuery);
            }
            else
            {
                foreach (var sortCriterion in queryRecord.SortCriteria)
                {
                    var state = FormParametersHelper.ToDynamic(sortCriterion.State);
                    groupQuery = this.AddSortCriterion(sortCriterion.Category, sortCriterion.Type, state, groupQuery);
                }
            }

            return groupQuery.Slice(skip, count);
        }
        public IHqlQuery AddSortCriterion(string category, string type, dynamic state, IHqlQuery groupQuery)
        {
            if (this.availableSortCriteria == null)
            {
                this.availableSortCriteria = this.DescribeSortCriteria().ToList();
            }

            var sortCriterionContext = new SortCriterionContext
            {
                Query = groupQuery,
                State = state
            };

            // look for the specific filter component
            var descriptor = availableSortCriteria.SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == category && x.Type == type);

            // ignore unfound descriptors
            if (descriptor == null)
            {
                return groupQuery;
            }

            // apply alteration
            descriptor.Sort(sortCriterionContext);

            return sortCriterionContext.Query;
        }

        public IHqlQuery ApplyFilter(IHqlQuery contentQuery, string category, string type, dynamic state)
        {
            if (this.availableFilters == null)
            {
                this.availableFilters = DescribeFilters().ToList();
            }

            // look for the specific filter component
            var descriptor = this.availableFilters
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(x => x.Category == category && x.Type == type);

            // ignore unfound descriptors
            if (descriptor == null)
            {
                return contentQuery;
            }

            var filterContext = new FilterContext
            {
                Query = contentQuery,
                State = state
            };

            // apply alteration
            descriptor.Filter(filterContext);

            return filterContext.Query;
        }

        public IEnumerable<IHqlQuery> GetContentQueries(QueryPartRecord queryRecord, IEnumerable<SortCriterionRecord> sortCriteria, IContent container)
        {
            var availableSortCriteria = DescribeSortCriteria().ToList();

            Dictionary<string, object> filtersDictionary = new Dictionary<string, object>();

            if (container != null)
            {
                filtersDictionary.Add("Content", container);
            }

            // pre-executing all groups 
            foreach (var group in queryRecord.FilterGroups)
            {
                var contentQuery = _contentManager.HqlQuery().ForVersion(VersionOptions.Published);

                // iterate over each filter to apply the alterations to the query object
                foreach (var filter in group.Filters)
                {
                    var tokenizedState = _tokenizer.Replace(filter.State, filtersDictionary);
                    dynamic state = FormParametersHelper.ToDynamic(tokenizedState);
                    contentQuery = this.ApplyFilter(contentQuery, filter.Category, filter.Type, state);
                }

                var sortModel = GetSortModelFromRequest(this.requestContext);

                if (sortModel != null)
                {
                    dynamic state = new Composite();
                    state.Sort = sortModel.Direction;
                    contentQuery = this.AddSortCriterion(sortModel.Category, sortModel.Type, state, contentQuery);
                }
                else if (sortCriteria != null)
                {
                    // iterate over each sort criteria to apply the alternatives to the query object
                    foreach (var sortCriterion in sortCriteria)
                    {
                        var state = FormParametersHelper.ToDynamic(sortCriterion.State);
                        contentQuery = this.AddSortCriterion(sortCriterion.Category, sortCriterion.Type, state, contentQuery);
                    }
                }

                yield return contentQuery;
            }
        }
    }
}