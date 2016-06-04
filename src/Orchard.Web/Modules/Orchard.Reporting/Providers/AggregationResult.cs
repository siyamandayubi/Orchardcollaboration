using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Providers
{
    public class AggregationResult
    {
        public string Label { get; set; }
        public double AggregationValue { get; set; }
        public object GroupingField { get; set; }
        public object Other { get; set; }
    }
}