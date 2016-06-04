using NHibernate.Transform;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Reporting.Models;
using Orchard.Reporting.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Services
{
    public class FieldAggregateQueryService : IFieldAggregateQueryService
    {
        private readonly Lazy<ISessionLocator> sessionLocator;

        public FieldAggregateQueryService(Lazy<ISessionLocator> sessionLocator)
        {
            this.sessionLocator = sessionLocator;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<AggregationResult> RunNumericAggregation(IHqlQuery query, AggregateMethods aggregateMethod, string fieldName, string partName, int interval)
        {
            string groupKey = string.Format(CultureInfo.InvariantCulture, "cast( floor(field.Value/{0}) as Double) ", interval.ToString(CultureInfo.InvariantCulture));
            string groupValue = string.Empty;
            switch (aggregateMethod)
            {
                case AggregateMethods.Count:
                    groupValue = "COUNT(*)";
                    break;
                case AggregateMethods.Sum:
                    groupValue = "SUM(field.Value)";
                    break;
                case AggregateMethods.Average:
                    groupValue = "AVG(field.Value)";
                    break;
                case AggregateMethods.Minimum:
                    groupValue = "MAX(field.Value)";
                    break;
                case AggregateMethods.Maximum:
                    groupValue = "MIN(field.Value)";
                    break;
                default:
                    throw new ArgumentException("Aggregate method is not provided");
            }

            var result = this.RunQuery(query, fieldName, partName, groupKey, groupValue, "DecimalFieldIndexRecords");

            List<AggregationResult> returnValue = new List<AggregationResult>();

            foreach (var group in result)
            {
                double value = double.Parse(group["GroupValue"].ToString());
                int? key = group["GroupKey"] != null ? (int?)int.Parse(group["GroupKey"].ToString()) : null;

                string keyLabel = !key.HasValue ? T("[{0}]", "Null").Text : T("{0} between {1}-{2}", fieldName, (key.Value * interval).ToString(CultureInfo.InvariantCulture), ((key.Value + 1) * interval).ToString(CultureInfo.InvariantCulture)).Text;

                AggregationResult aggregationResult = new AggregationResult
                {
                    AggregationValue = value,
                    Label = keyLabel,
                    GroupingField = key
                };

                returnValue.Add(aggregationResult);
            }

            return returnValue;
        }

        public IEnumerable<AggregationResult> RunEnumerationAggregation(IHqlQuery query, AggregateMethods aggregateMethod, string fieldName, string partName)
        {
            string groupKey = "field.Value";
            string groupValue = string.Empty;
            switch (aggregateMethod)
            {
                case AggregateMethods.Count:
                    groupValue = "COUNT(*)";
                    break;
                default:
                    throw new ArgumentException("Aggregate method is not provided");
            }

            var result = this.RunQuery(query, fieldName, partName, groupKey, groupValue, "StringFieldIndexRecords");

            List<AggregationResult> returnValue = new List<AggregationResult>();

            foreach (var group in result)
            {
                double value = double.Parse(group["GroupValue"].ToString());
                string key = group["GroupKey"] != null ? group["GroupKey"].ToString() : string.Empty;

                string keyLabel = string.IsNullOrEmpty(key) || key == T("Select an option").Text ? T("[{0}]", "Null").Text : key;

                AggregationResult aggregationResult = new AggregationResult
                {
                    AggregationValue = value,
                    Label = keyLabel,
                    GroupingField = key
                };

                returnValue.Add(aggregationResult);
            }

            return returnValue;
        }

        private IList<IDictionary> RunQuery(IHqlQuery query, string fieldName, string partName, string groupKey, string groupValue, string fieldTableName)
        {
            DefaultHqlQuery defaultQuery = query as DefaultHqlQuery;
            var queryHql = defaultQuery.ToHql(true);

            var hql = @"select {0} as GroupKey, {1} as GroupValue
                        from Orchard.ContentManagement.Records.ContentItemVersionRecord as kiv
                        join kiv.ContentItemRecord as ki
                        join ki.FieldIndexPartRecord as fieldIndexPartRecord
                        join fieldIndexPartRecord.{5} as field
                        where (field.PropertyName = '{2}.{3}.') AND (kiv.Published = True) AND kiv.Id in ({4})
                        group by {0}";

            hql = string.Format(CultureInfo.InvariantCulture, hql, groupKey, groupValue, partName, fieldName, queryHql, fieldTableName);

            var session = this.sessionLocator.Value.For(typeof(ContentItem));
            return session
                   .CreateQuery(hql)
                   .SetCacheable(false)
                   .SetResultTransformer(Transformers.AliasToEntityMap)
                   .List<IDictionary>();

        }
    }
}