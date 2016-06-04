using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement.Shapes;
using Orchard.Forms.Services;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Reporting.Models;
using Orchard.Reporting.Providers;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Services
{
    public class ReportManager : IReportManager
    {
        private readonly IEnumerable<IGroupByParameterProvider> groupByProviders;
        private readonly IContentManager contentManager;
        private readonly IProjectionManager projectionManager;
        private readonly ITokenizer _tokenizer;
        private readonly IRepository<QueryPartRecord> queryRepository;

        public ReportManager(
            IRepository<QueryPartRecord> queryRepository,
            IProjectionManager projectionManager,
            IEnumerable<IGroupByParameterProvider> groupByProviders,
            IContentManager contentManager,
            ITokenizer tokenizer)
        {
            this.queryRepository = queryRepository;
            this.projectionManager = projectionManager;
            this._tokenizer = tokenizer;
            this.contentManager = contentManager;
            this.groupByProviders = groupByProviders;
        }

        public IEnumerable<TypeDescriptor<GroupByDescriptor>> DescribeGroupByFields()
        {
            DescribeGroupByContext context = new DescribeGroupByContext();
            foreach (var provider in this.groupByProviders)
            {
                provider.Describe(context);
            }

            return context.Describe();
        }

        public int GetCount(ReportRecord report, IContent container)
        {
            if (report == null) { throw new ArgumentNullException("report"); }
            if (report.Query == null) { throw new ArgumentException("There is no QueryRecord associated with the Report"); }

            var descriptors = this.DescribeGroupByFields();
            var descriptor = descriptors.SelectMany(c => c.Descriptors).FirstOrDefault(c => c.Category == report.GroupByCategory && c.Type == report.GroupByType);

            if (descriptor == null)
            {
                throw new ArgumentOutOfRangeException("There is no GroupByDescriptor for the given category and type");
            }

            var queryRecord = this.queryRepository.Get(report.Query.Id);

            var contentQueries = this.GetContentQueries(queryRecord, queryRecord.SortCriteria, container);

            return contentQueries.Sum(c => c.Count());
        }

        public IEnumerable<AggregationResult> RunReport(ReportRecord report, IContent container)
        {
            if (report == null) { throw new ArgumentNullException("report"); }
            if (report.Query == null) { throw new ArgumentException("There is no QueryRecord associated with the Report"); }

            var descriptors = this.DescribeGroupByFields();
            var descriptor = descriptors.SelectMany(c => c.Descriptors).FirstOrDefault(c => c.Category == report.GroupByCategory && c.Type == report.GroupByType);

            if (descriptor == null)
            {
                throw new ArgumentOutOfRangeException("There is no GroupByDescriptor for the given category and type");
            }

            var queryRecord = this.queryRepository.Get(report.Query.Id);

            var contentQueries = this.GetContentQueries(queryRecord, queryRecord.SortCriteria, container);

            Dictionary<string, AggregationResult> returnValue = new Dictionary<string, AggregationResult>();

            foreach (var contentQuery in contentQueries)
            {
                var dictionary = descriptor.Run(contentQuery, (AggregateMethods)report.AggregateMethod);

                foreach (var item in dictionary)
                {
                    if (returnValue.ContainsKey(item.Label))
                    {
                        var previousItem = returnValue[item.Label];
                        previousItem.AggregationValue += item.AggregationValue;
                        returnValue[item.Label] = previousItem;
                    }
                    else
                    {
                        returnValue[item.Label] = item;
                    }
                }
            }

            return returnValue.Values;
        }

        public IHqlQuery ApplyFilter(IHqlQuery contentQuery, string category, string type, dynamic state)
        {
            var availableFilters = projectionManager.DescribeFilters().ToList();

            // look for the specific filter component
            var descriptor = availableFilters
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
            Dictionary<string, object> filtersDictionary = new Dictionary<string, object>();

            if (container != null)
            {
                filtersDictionary.Add("Content", container);
            }
            
            // pre-executing all groups 
            foreach (var group in queryRecord.FilterGroups)
            {
                var contentQuery = this.contentManager.HqlQuery().ForVersion(VersionOptions.Published);

                // iterate over each filter to apply the alterations to the query object
                foreach (var filter in group.Filters)
                {
                    var tokenizedState = _tokenizer.Replace(filter.State, filtersDictionary);
                    dynamic state = FormParametersHelper.ToDynamic(tokenizedState);
                    contentQuery = this.ApplyFilter(contentQuery, filter.Category, filter.Type, state);
                }

                yield return contentQuery;
            }
        }

    }
}