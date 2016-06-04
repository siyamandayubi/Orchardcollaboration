using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Reporting.ViewModels
{
    public class EditDataReportViewerViewModel
    {
        private Collection<SelectListItem> reports = new Collection<SelectListItem>();

        public Collection<SelectListItem> Reports
        {
            get
            {
                return this.reports;
            }
        }

        [Required]
        public int? ReportId { get; set; }

        public string ContainerCssClass { get; set; }
        public string ChartTagCssClass { get; set; }
    }
}