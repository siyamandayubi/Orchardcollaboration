namespace Orchard.CRM.Core.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    public class ActivityStreamEditModel
    {
        private Collection<SelectListItem> queries = new Collection<SelectListItem>();

        public Collection<SelectListItem> Queries { get { return this.queries; } }

        public int PageSize { get; set; }

        public bool ShowPager { get; set; }

        public int? SelectedQueryId { get; set; }
    }
}