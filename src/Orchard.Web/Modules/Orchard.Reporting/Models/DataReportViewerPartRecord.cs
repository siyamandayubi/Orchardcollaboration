using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Projections.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Models
{
    public class DataReportViewerPartRecord : ContentPartRecord
    {
        [Aggregate]
        public virtual ReportRecord Report { get; set; }

        public virtual string ContainerTagCssClass { get; set; }

        public virtual string ChartTagCssClass { get; set; }
    }
}