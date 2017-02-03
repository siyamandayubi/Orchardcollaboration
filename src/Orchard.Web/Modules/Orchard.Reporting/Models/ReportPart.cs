using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Models
{
    public class ReportPart : ContentPart
    {
        public int ReportId
        {
            get { return this.Retrieve(x => x.ReportId); }
            set { this.Store(x => x.ReportId, value); }
        }

    }
}