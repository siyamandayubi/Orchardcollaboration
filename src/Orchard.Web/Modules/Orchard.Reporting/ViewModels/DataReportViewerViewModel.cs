using Orchard.Reporting.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.ViewModels
{
    public class DataReportViewerViewModel
    {
        public int ChartType { get; set; }
        public int TotalCount { get; set; }
        public string JsonData { get; set; }
        public string ReportTitle { get; set; }
        public string ContainerCssClass { get; set; }
        public string ChartCssClass { get; set; }
        public List<AggregationResult> Data { get; set; }
    }
}