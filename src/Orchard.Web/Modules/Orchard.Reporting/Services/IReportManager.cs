using Orchard.ContentManagement;
using Orchard.Projections.Descriptors;
using Orchard.Reporting.Models;
using Orchard.Reporting.Providers;
using System.Collections.Generic;

namespace Orchard.Reporting.Services
{
    public interface IReportManager : IDependency
    {
        IEnumerable<TypeDescriptor<GroupByDescriptor>> DescribeGroupByFields();
        ReportResult RunReport(ReportRecord report, IContent container);
        int GetCount(ReportRecord report, IContent container);
    }
}
