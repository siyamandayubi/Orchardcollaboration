using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Drivers
{
    public class DashboardDriver : CRMContentPartDriver<DashboardPart>
    {
        private readonly IBasicDataService basicDataService;
        private readonly IGroupQuery groupQuery;
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort;

        public DashboardDriver(
            ICRMContentOwnershipService crmContentOwnershipService,
            IOrchardServices orchardServices,
            IGroupQuery groupQuery,
            IBasicDataService basicDataService,
            IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort
            )
            : base(orchardServices)
        {
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.groupQuery = groupQuery;
            this.projectionManagerWithDynamicSort = projectionManagerWithDynamicSort;
            this.basicDataService = basicDataService;
        }

        protected override DriverResult Display(DashboardPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType == "SummaryAdmin" || displayType == "Summary")
            {
                return ContentShape("Parts_Dashboard_Summary",
                      () => shapeHelper.Parts_Dashboard_Summary(
                          Model: part
                          ));
            }
            else
            {
                return this.DisplayDetail(part, shapeHelper);
            }
        }

        private DriverResult DisplayDetail(DashboardPart part, dynamic shapeHelper)
        {
            if (this.services.WorkContext.CurrentUser == null)
            {
                return null;
            }

            var contentQuery = this.services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);
            var statusRecords = this.basicDataService.GetStatusRecords().OrderBy(c => c.OrderId).ToList();

            DashboardViewModel model = new DashboardViewModel();
            model.CurrentUserId = this.services.WorkContext.CurrentUser.Id;
            model.IsCustomer = this.crmContentOwnershipService.IsCurrentUserCustomer();
            model.IsOperator = this.services.Authorizer.Authorize(Permissions.OperatorPermission);
            dynamic state = new JObject();

            // Query items created by customer
            if (model.IsCustomer)
            {
                // Ticket contentType
                state.ContentTypes = "Ticket";
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);

                state.RequestingUser_Id = model.CurrentUserId.ToString(CultureInfo.InvariantCulture);
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.RequestingUserType, state);

                var userTicketsCountByStateIds = groupQuery.GetCount(contentQuery, "TicketPartRecord", "StatusRecord.Id");

                model.CurrentUserRequestingTickets = new Collection<dynamic>();
                CRMHelper.AddStatusGroupRecordsToModel(statusRecords, userTicketsCountByStateIds, model.CurrentUserRequestingTickets);

                // overrude items of current users
                state.MaxDueDate = DateTime.UtcNow.Date;
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.TicketDueDateType, state);
                model.CurrentUserOverrudeRequestingTicketCount = contentQuery.Count();
            }

            // Query the counts of the current user tickets group by stateId
            // *******************************************************
            if (model.IsOperator)
            {
                // Ticket contentType
                state.ContentTypes = "Ticket";
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);

                dynamic temp = new
                {
                    Users = new int[] { model.CurrentUserId },
                    Teams = new int[] { },
                    BusinessUnits = new int[] { },
                    AccessType = ContentItemPermissionAccessTypes.Assignee
                };

                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.AnySelectedUserTeamBusinessUnit, temp);

                var userTicketsCountByStateIds = groupQuery.GetCount(contentQuery, "TicketPartRecord", "StatusRecord.Id");

                model.CurrentUserTickets = new Collection<dynamic>();
                CRMHelper.AddStatusGroupRecordsToModel(statusRecords, userTicketsCountByStateIds, model.CurrentUserTickets);

                // overrude items of current users
                state.MaxDueDate = DateTime.UtcNow.Date;
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.TicketDueDateType, state);
                model.CurrentUserOverrudeItemsCount = contentQuery.Count();
                //*******************************************************
            }

            bool isAdmin = this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);

            if (isAdmin)
            {
                // Query the counts of the whole tickets in the system based on stateId
                state = new JObject();

                contentQuery = this.services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);

                state.ContentTypes = "Ticket";
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);
                var ticketCountsByStateIds = groupQuery.GetCount(contentQuery, "TicketPartRecord", "StatusRecord.Id");

                model.AllTickets = new Collection<dynamic>();
                CRMHelper.AddStatusGroupRecordsToModel(statusRecords, ticketCountsByStateIds, model.AllTickets);

                state.MaxDueDate = DateTime.UtcNow.Date;
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.TicketDueDateType, state);
                model.AllOverrudeItemsCount = contentQuery.Count();
            }

            // get items without any owner
            contentQuery = this.services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);
            state.ContentTypes = "Ticket";
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, "ContentItemPermissionPartRecord.ItemsWithoutAnyOwner", state);
            model.AllItemsWithoutOwnerCount = contentQuery.Count();

            // get overrude items count

            // display
            // 1) Number of your open, new, in progress, and closed tickets
            // 2) number of the unassigned, new, open, in progress and closed tickets in the system.
            return ContentShape("Parts_Dashboard",
                () => shapeHelper.Parts_Dashboard(
                    Model: model
                    ));
        }
    }
}