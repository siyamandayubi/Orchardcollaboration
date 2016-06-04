using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.ViewModels
{
    public class ReportListViewModel
    {
        private Collection<ReportViewModel> reports = new Collection<ReportViewModel>();

        public Collection<ReportViewModel> Reports
        {
            get { return this.reports; }
        }

        public dynamic Pager { get; set; }
    }
}