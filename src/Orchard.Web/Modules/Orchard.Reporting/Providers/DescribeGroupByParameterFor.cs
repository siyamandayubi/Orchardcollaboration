using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Providers
{
    public class DescribeGroupByParameterFor
    {
        private readonly string _category;
        public DescribeGroupByParameterFor(string category, LocalizedString name, LocalizedString description)
        {
            Types = new List<GroupByDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<GroupByDescriptor> Types { get; private set; }

        public DescribeGroupByParameterFor Element(string type, LocalizedString name, LocalizedString description, Func<IHqlQuery, AggregateMethods, IEnumerable<AggregationResult>> run, Func<FilterContext, LocalizedString> display, IList<AggregateMethods> aggregateMethods)
        {
            var groupByDescriptor = new GroupByDescriptor { Type = type, Name = name, Description = description, Category = _category, Run= run, Display = display};
            groupByDescriptor.AggregateMethods.AddRange(aggregateMethods);
            Types.Add(groupByDescriptor);
            return this;
        }
  }
}