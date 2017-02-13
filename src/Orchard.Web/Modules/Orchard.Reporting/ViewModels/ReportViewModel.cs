using Orchard.Projections.Descriptors;
using Orchard.Reporting.Providers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Reporting.ViewModels
{
    public class ReportViewModel
    {
        private Collection<SelectListItem> queries = new Collection<SelectListItem>();
        private Collection<SelectListItem> chartTypes = new Collection<SelectListItem>();
        private Collection<ReportGroupByFieldCollectionViewModel> groupByFieldsCollection = new Collection<ReportGroupByFieldCollectionViewModel>();
        private Collection<SelectListItem> aggregations = new Collection<SelectListItem>();

        public Collection<ReportGroupByFieldCollectionViewModel> GroupByFieldsCollection
        {
            get
            {
                return this.groupByFieldsCollection;
            }
        }

        public Collection<SelectListItem> Aggregations
        {
            get
            {
                return this.aggregations;
            }
        }

        public Collection<SelectListItem> Queries
        {
            get
            {
                return this.queries;
            }
        }

        public Collection<SelectListItem> ChartTypes
        {
            get
            {
                return this.chartTypes;
            }
        }

        public int ReportId { get; set; }
        
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }

        [MaxLength(100)]
        [Required]
        public string Title { get; set; }

        [MaxLength(500)]
        [Required]
        public string State { get; set; }

        [MaxLength(202)]
        [Required]
        public string CategoryAndType { get; set; }

        [Required]
        public int ChartTypeId { get; set; }

        [Required]
        public int AggregateMethod { get; set; }

        [Required]
        public int? QueryId { get; set; }
    }
}