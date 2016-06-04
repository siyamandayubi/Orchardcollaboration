using Orchard.ContentManagement.MetaData.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Settings
{
    public class DataReportViewerPartSettings
    {
        public int? DefaultReportId { get; set; }

        public virtual string ContainerTagCssClass { get; set; }

        public virtual string ChartTagCssClass { get; set; }
 
        public void Build(ContentTypePartDefinitionBuilder builder)
        {
            builder.WithSetting("DataReportViewerSettings.DefaultReportId", DefaultReportId.HasValue ? DefaultReportId.ToString() : string.Empty);
            builder.WithSetting("DataReportViewerSettings.ContainerTagCssClass", ContainerTagCssClass);
            builder.WithSetting("DataReportViewerSettings.DefaultReportId", ChartTagCssClass);
        }
    }
}