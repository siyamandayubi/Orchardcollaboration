using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.CRM.Dashboard.Models;
using S22.IMAP.Models;
using Orchard.ContentManagement;
using Newtonsoft.Json.Linq;
using Orchard.CRM.Dashboard.Services;

namespace Orchard.CRM.Dashboard.Drivers
{
    public class DashboardManagementDriver : ContentPartDriver<DashboardManagementPart>
    {
        protected readonly IOrchardServices orchardServices;
        private readonly IQueriesAndProjectionsGenerator queriesAndProjectionsGenerator;

        public DashboardManagementDriver(IOrchardServices orchardServices, IQueriesAndProjectionsGenerator queriesAndProjectionsGenerator)
        {
            this.queriesAndProjectionsGenerator = queriesAndProjectionsGenerator;
            this.orchardServices = orchardServices;
        }

        protected override DriverResult Display(DashboardManagementPart part, string displayType, dynamic shapeHelper)
        {
            var count = this.orchardServices.ContentManager.HqlQuery().ForType(Consts.GenericDashboardContentType).Count();
            dynamic model = new JObject();
            model.Count = count;

            if (part.DashboardListContentId == default(int))
            {
                var projection = CreateDashboardProjection();
                part.DashboardListContentId = projection.Id;
            }

            model.ListContentItemId = part.DashboardListContentId;

            return ContentShape("Parts_DashboardManagement_Summary", () => shapeHelper.Parts_DashboardManagement_Summary(Model: model));
        }

        private ContentItem CreateDashboardProjection()
        {
            var query = this.queriesAndProjectionsGenerator.GetQuery(QueryNames.DashboardList);

            if (query == null)
            {
                query = this.queriesAndProjectionsGenerator.CreateQuery(QueryNames.DashboardList, Consts.GenericDashboardContentType, "DashboardList", "Shape", "Summary", true, false);
            }

            var projection = this.queriesAndProjectionsGenerator.CreateProjection("ProjectionWithDynamicSortPage", QueryNames.DashboardList, "Dashboards", Consts.GenericDashboardContentType);

            return projection;
        }
    }
}