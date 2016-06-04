using Orchard.ContentManagement;
using Orchard.Reporting.Models;
using Orchard.Reporting.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Reporting.Services
{
    public interface IFieldAggregateQueryService : IDependency
    {
        IEnumerable<AggregationResult> RunNumericAggregation(IHqlQuery query, AggregateMethods aggregateMethod, string fieldName, string partName, int interval);
        IEnumerable<AggregationResult> RunEnumerationAggregation(IHqlQuery query, AggregateMethods aggregateMethod, string fieldName, string partName);
    }
}
