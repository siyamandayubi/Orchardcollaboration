using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class ActivityStreamPart : ContentPart
    {
        public int? QueryId
        {
            get { return this.Retrieve(x => x.QueryId); }
            set { this.Store(x => x.QueryId, value); }
        }

        public int PageSize
        {
            get { return this.Retrieve(x => x.PageSize); }
            set { this.Store(x => x.PageSize, value); }
        }

        public bool ShowPager
        {
            get { return this.Retrieve(x => x.ShowPager); }
            set { this.Store(x => x.ShowPager, value); }
        }
    }
}