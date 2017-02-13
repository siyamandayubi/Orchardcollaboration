namespace Orchard.CRM.Core.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class EditPostedActivityStreamViewModel
    {
        public int? SelectedQueryId { get; set; }
        public int PageSize { get; set; }
        public bool ShowPager { get; set; }
    }
}