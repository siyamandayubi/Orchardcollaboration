/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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