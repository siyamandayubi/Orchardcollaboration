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

namespace Orchard.CRM.Core.Drivers
{
    using System.Linq;
    using Orchard.ContentManagement.Drivers;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.UI.Navigation;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Dynamic;
    using System;
    using Orchard.Localization;
    using Orchard.Data;
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.Projections.Models;
    using System.Web.WebPages.Html;
    using Orchard.Core.Title.Models;
    using System.Globalization;
    using System.Web.Routing;
    using Orchard.CRM.Core.Settings;

    public class ActivityStreamPartDriver : CRMContentPartDriver<ActivityStreamPart>
    {
        private readonly IActivityStreamService activityStreamService;
        private readonly IBasicDataService basicDataService;
        private readonly IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort;
        private readonly Lazy<ISessionLocator> sessionLocator;

        public ActivityStreamPartDriver(
            IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort,
            IOrchardServices services,
            Lazy<ISessionLocator> sessionLocator,
            IBasicDataService basicDataService,
            IActivityStreamService activityStreamService)
            : base(services)
        {
            this.projectionManagerWithDynamicSort = projectionManagerWithDynamicSort;
            this.basicDataService = basicDataService;
            this.activityStreamService = activityStreamService;
            this.sessionLocator = sessionLocator;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(ActivityStreamPart part, string displayType, dynamic shapeHelper)
        {
            if (this.services.WorkContext.CurrentUser == null)
            {
                return null;
            }

            // retrieving paging parameters
            var queryString = this.services.WorkContext.HttpContext.Request.QueryString;

            var settings = part.TypePartDefinition.Settings.GetModel<ActivityStreamPartSettings>();

            var pageKey = "page";
            var page = 0;
            int pageSize = settings.PageSize != default(int) ? settings.PageSize : 20;

            // don't try to page if not necessary
            if (queryString.AllKeys.Contains(pageKey))
            {
                int.TryParse(queryString[pageKey], out page);
            }

            int count = 0;
            List<ActivityStreamRecord> items = new List<ActivityStreamRecord>();

            QueryPart queryPart = part.QueryId.HasValue ? this.services.ContentManager.Get<QueryPart>(part.QueryId.Value) : null;
            if (queryPart != null)
            {
                var queries = projectionManagerWithDynamicSort.GetContentQueries(queryPart.Record, null, part.ContentItem).ToArray();

                count += this.activityStreamService.ActivityStreamRestrictedByGivenQueryCount(queries);
                items.AddRange(this.activityStreamService.ActivityStreamRestrictedByGivenQuery(queries, page == 0 ? page : page - 1, pageSize));

                items = items.OrderByDescending(c => c.Id).Take(pageSize).ToList();
            }
            else
            {
                count = this.activityStreamService.ActivityStreamVisibleByCurrentUserCount();
                items = this.activityStreamService.ActivityStreamVisibleByCurrentUser(page == 0 ? page : page - 1, pageSize).ToList();
            }

            var model = this.activityStreamService.CreateModel(items, count, page, pageSize);

            return this.ContentShape("Parts_DashboardActivityStream",
                () => shapeHelper.Parts_DashboardActivityStream(Model: model));
        }

        protected override DriverResult Editor(ActivityStreamPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            EditPostedActivityStreamViewModel model = new EditPostedActivityStreamViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            part.QueryId = model.SelectedQueryId;
            part.PageSize = model.PageSize;
            part.ShowPager = model.ShowPager;

            return this.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(ActivityStreamPart part, dynamic shapeHelper)
        {
            ActivityStreamEditModel model = new ActivityStreamEditModel();
            var queries = this.services.ContentManager.Query<QueryPart>().ForVersion(VersionOptions.Published).List();

            model.Queries.AddRange(queries.Select(c => new System.Web.Mvc.SelectListItem
            {
                Text = c.As<TitlePart>().Title,
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Selected = c.Id == part.QueryId
            }));

            model.PageSize = part.PageSize;
            model.SelectedQueryId = part.QueryId;
            model.ShowPager = part.ShowPager;

            return ContentShape("Parts_ActivityStream_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/ActivityStream",
                        Model: model,
                        Prefix: Prefix));
        }
    }
}