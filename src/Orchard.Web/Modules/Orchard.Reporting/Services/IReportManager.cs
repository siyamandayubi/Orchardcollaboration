using Orchard.ContentManagement;
using Orchard.Projections.Descriptors;
using Orchard.Reporting.Models;
using Orchard.Reporting.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Reporting.Services
{
    public interface IReportManager: IDependency
    {
        IEnumerable<TypeDescriptor<GroupByDescriptor>> DescribeGroupByFields();
        IEnumerable<AggregationResult> RunReport(ReportRecord report, IContent container);
        int GetCount(ReportRecord report, IContent container);
    }
}
