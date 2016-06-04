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
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Drivers;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.Settings;
    using Orchard.CRM.Core.ViewModels;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using System.Linq;

    public class TicketsForContentItemDriver : CRMContentPartDriver<TicketsForContentItemPart>
    {
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly IBasicDataService basicDataService;
        private readonly IGroupQuery groupQuery;
        private readonly IWorkContextAccessor workContextAccessor;
        private readonly ISearchTicketService searchTicketService;

        public const string FlipFlopShapeName = "TicketsForContentItemFlipFlop";

        public TicketsForContentItemDriver(
            IWorkContextAccessor workContextAccessor,
            IOrchardServices services,
            ISearchTicketService searchTicketService,
            ICRMContentOwnershipService crmContentOwnershipService,
            IGroupQuery groupQuery,
            IBasicDataService basicDataService)
            : base(services)
        {
            this.searchTicketService = searchTicketService;
            this.workContextAccessor = workContextAccessor;
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.groupQuery = groupQuery;
            this.basicDataService = basicDataService;
        }

        protected override DriverResult Display(TicketsForContentItemPart part, string displayType, dynamic shapeHelper)
        {
            if (this.crmContentOwnershipService.IsCurrentUserOperator() && displayType == "Detail")
            {
                var context = workContextAccessor.GetContext();
                var layout = context.Layout;

                // for the ticket related pages, we don't render any menu
                if (layout.TicketRelatedPage != null && layout.TicketRelatedPage == true)
                {
                    return null;
                }

                var settings = part.TypePartDefinition.Settings.GetModel<TicketsForContentItemPartSettings>();

                if (!settings.DoNotRenderTicketsFlipFlop)
                {
                    this.RenderFlipFlop(shapeHelper);
                }

                var statusRecords = this.basicDataService.GetStatusRecords().OrderBy(c => c.OrderId).ToList();

                var contentQuery = searchTicketService.CreateQuery(new PostedTicketSearchViewModel
                {
                    Users = new int[] { },
                    BusinessUnits = new int[] { },
                    RelatedContentItemId = part.ContentItem.Id,
                    IncludeAllVisibleItemsBySelectedGroupsAndUsers = true
                });

                var ticketsCountByStateIds = groupQuery.GetCount(contentQuery, "TicketPartRecord", "StatusRecord.Id");

                var ticketsCountWithLabels = new Collection<dynamic>();
                CRMHelper.AddStatusGroupRecordsToModel(statusRecords, ticketsCountByStateIds, ticketsCountWithLabels);

                // overrude items of current users
                var overrudeTicketCount = searchTicketService.CountByDatabase(new PostedTicketSearchViewModel
                {
                    Users = new int[] { },
                    BusinessUnits = new int[] { },
                    RelatedContentItemId = part.ContentItem.Id,
                    DueDate = PostedTicketSearchViewModel.OverDueDate,
                    IncludeAllVisibleItemsBySelectedGroupsAndUsers = true
                });

                var unassignedTicketCount = searchTicketService.CountByDatabase(new PostedTicketSearchViewModel
                {
                    Users = new int[] { },
                    BusinessUnits = new int[] { },
                    RelatedContentItemId = part.ContentItem.Id,
                    Unassigned = true
                });

                dynamic model = new ExpandoObject();
                model.TicketsGroupByStatus = ticketsCountWithLabels;
                model.OverrudeTicketCount = overrudeTicketCount;
                model.UnAssignedTicketCount = unassignedTicketCount;
                model.Part = part;
                return ContentShape("Parts_TicketsForContentItem",
                    () => shapeHelper.Parts_TicketsForContentItem(
                        Model: model
                        ));
            }
            else
            {
                return null;
            }
        }

        private void RenderFlipFlop(dynamic shapeHelper)
        {
            var context = workContextAccessor.GetContext();

            var layout = context.Layout;
            bool flipFlopIsRendered = false;
            foreach (var item in layout.Body.Items)
            {
                if (item.TokenName == FlipFlopShapeName)
                {
                    flipFlopIsRendered = true;
                    break;
                }
            }

            if (!flipFlopIsRendered)
            {
                var ticketSettings = this.services.WorkContext.CurrentSite.As<TicketSettingPart>();
                var shape = shapeHelper.TicketsForContentItemFlipFlop(Model: ticketSettings.TicketsForContentItemsMenuFlipFlopPosition);
                shape.TokenName = FlipFlopShapeName;
                context.Layout.Body.Add(shape);
            }
        }
    }
}