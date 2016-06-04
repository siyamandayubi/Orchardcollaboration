using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Handlers
{
    public class DataReportViewerHandler : ContentHandler
    {
        public DataReportViewerHandler(IRepository<DataReportViewerPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}