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