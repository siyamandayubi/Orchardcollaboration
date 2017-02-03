using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public class HelperService : IHelperService
    {
        private readonly IOrchardServices services;

        public HelperService(IOrchardServices services)
        {
            this.services = services;
        }
        
        public Pager ReterivePagerFromQueryString()
        {
            // retrieving paging parameters
            var queryString = this.services.WorkContext.HttpContext.Request.QueryString;

            var pageKey = "page";
            var page = 0;

            // default page size
            int pageSize = 10;

            if (queryString.AllKeys.Contains(pageKey))
            {
                Int32.TryParse(queryString[pageKey], out page);
            }

            // if 0, then assume "All", limit to 128 by default
            if (pageSize == 128)
            {
                pageSize = Int32.MaxValue;
            }

            var pager = new Pager(this.services.WorkContext.CurrentSite, page, pageSize);

            return pager;
        }
    }
}