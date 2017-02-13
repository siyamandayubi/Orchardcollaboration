using Orchard.Reporting.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Providers
{
    public class ReportResult
    {
        public ReportResult()
        {
            Items = new List<AggregationResult>();
            Other = new Dictionary<string, object>();
        }

        public IDictionary<string,object> Other { get; private set; }
        public List<AggregationResult> Items { get; private set; }
    }
}