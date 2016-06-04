/// Orchard Collaboration is a plugin for Orchard CMS that provides an integrated ticketing system for it.
/// Copyright (C) 2014-2015  Siyamand Ayubi
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

namespace Orchard.CRM.Core.Drivers
{
    using System.Linq;
    using Orchard.ContentManagement.Drivers;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.UI.Navigation;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public class DashboardActivityStreamDriver : CRMContentPartDriver<DashboardActivityStreamPart>
    {
        private readonly IActivityStreamService activityStreamService;

        public DashboardActivityStreamDriver(
            IOrchardServices services,
            IActivityStreamService activityStreamService)
            : base(services)
        {
            this.activityStreamService = activityStreamService;
        }

        protected override DriverResult Display(DashboardActivityStreamPart part, string displayType, dynamic shapeHelper)
        {
            // retrieving paging parameters
            var queryString = this.services.WorkContext.HttpContext.Request.QueryString;

            var pageKey = "page";
            var page = 0;
            int pageSize = 50;

            // don't try to page if not necessary
            if (queryString.AllKeys.Contains(pageKey))
            {
                int.TryParse(queryString[pageKey], out page);
            }

            int count = this.activityStreamService.GetItemsVisibleByCurrentUserCount();
            var items = this.activityStreamService.GetItemsVisibleByCurrentUser(page, pageSize);

            var pager = new Pager(this.services.WorkContext.CurrentSite, page, pageSize);

            dynamic model = new JObject();
            model.Pager = pager;
            model.Items = items;

            return this.ContentShape("Parts_DashboardActivityStream",
                () => shapeHelper.Parts_DashboardActivityStream(Model: model));
        }
    }
}