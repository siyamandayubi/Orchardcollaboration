using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Drivers
{
    public class TicketMenuItemDriver : CRMContentPartDriver<TicketMenuItemPart>
    {
        private readonly ICRMContentOwnershipService contentOwnershipService;
        private readonly IBasicDataService basicDataService;
        protected readonly IContentOwnershipHelper contentOwnershipHelper;

        public TicketMenuItemDriver(
            IOrchardServices services,
            IContentOwnershipHelper contentOwnershipHelper,
            ICRMContentOwnershipService contentOwnershipService,
            IBasicDataService basicDataService)
            : base(services)
        {
            this.contentOwnershipHelper = contentOwnershipHelper;
            this.basicDataService = basicDataService;
            this.contentOwnershipService = contentOwnershipService;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(TicketMenuItemPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return null;
            }

            TicketMenuItemPostViewModel viewModel = new TicketMenuItemPostViewModel();
            updater.TryUpdateModel(viewModel, Prefix, null, null);

            var businessUnitAndTeamIds = Converter.DecodeGroupId(viewModel.GroupId);
            if (businessUnitAndTeamIds != null)
            {
                if (businessUnitAndTeamIds.TeamId.HasValue)
                {
                    part.Record.Team = new TeamPartRecord { Id = businessUnitAndTeamIds.TeamId.Value };
                    part.Record.BusinessUnit = null;
                    part.Record.User = null;
                }
                else if (businessUnitAndTeamIds.BusinessUnitId.HasValue)
                {
                    part.Record.BusinessUnit = new BusinessUnitPartRecord { Id = businessUnitAndTeamIds.BusinessUnitId.Value };
                    part.Record.User = null;
                    part.Record.Team = null;
                }
            }
            else
            {
                part.Record.BusinessUnit = null;
                part.Record.Team = null;
            }

            if (viewModel.UserId.HasValue)
            {
                part.Record.User = new UserPartRecord { Id = viewModel.UserId.Value };
                part.Record.BusinessUnit = null;
                part.Record.Team = null;
            }
            else
            {
                part.Record.User = null;
            }

            part.Record.Status = viewModel.StatusId.HasValue ? new StatusRecord { Id = viewModel.StatusId.Value } : null;
            part.Record.DueDateDays = viewModel.DueDateDays;

            return this.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(TicketMenuItemPart part, dynamic shapeHelper)
        {
            var statusRecords = this.basicDataService.GetStatusRecords().ToList().Select(c => new BasicDataRecordViewModel { Id = c.Id, Name = c.Name }).ToList();
            Collection<SelectListItem> statusSelectList = new Collection<SelectListItem>();

            int? statusId = part.Record.Status != null ? (int?)part.Record.Status.Id : null;
            Converter.Fill(statusSelectList, statusId, statusRecords);
            statusSelectList.Insert(0, new SelectListItem());

            Collection<SelectListItem> dueDates = CRMHelper.GetDueDateItems(this.T);
            if (part.Record.DueDateDays.HasValue)
            {
                dueDates
                    .Where(c => c.Value == part.Record.DueDateDays.Value.ToString(CultureInfo.InvariantCulture))
                    .ToList()
                    .ForEach(c => c.Selected = true);
            }

            ContentItemSetPermissionsViewModel permissionsViewModel = this.contentOwnershipHelper.CreateModel();
            permissionsViewModel.Users.Insert(0, new SelectListItem());
            if (part.Record.User != null)
            {
                permissionsViewModel
                    .Users
                    .Where(c => c.Value == part.Record.User.Id.ToString(CultureInfo.InvariantCulture))
                    .ToList()
                    .ForEach(c => c.Selected = true);
            }

            if (part.Record.BusinessUnit != null)
            {
                permissionsViewModel
                    .BusinessUnits
                    .Where(c => c.BusinessUnitId == part.Record.BusinessUnit.Id)
                    .ToList()
                    .ForEach(c => c.Checked = true);
            }

            if (part.Record.Team != null)
            {
                permissionsViewModel
                    .Teams
                    .Where(c => c.TeamId == part.Record.Team.Id)
                    .ToList()
                    .ForEach(c => c.Checked = true);
            }

            return ContentShape("Parts_TicketMenuItem_Edit",
                                   () =>
                                   {
                                       var model = new TicketMenuItemViewModel(
                                           statusSelectList,                                           
                                           dueDates,
                                           permissionsViewModel.Users,
                                           permissionsViewModel.BusinessUnits,
                                           permissionsViewModel.Teams);

                                       return shapeHelper.EditorTemplate(TemplateName: "Parts/TicketMenuItem", Model: model, Prefix: Prefix);
                                   });
        }
    }
}