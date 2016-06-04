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
    using Orchard.ContentManagement.Handlers;
    using Orchard.ContentManagement.Records;
    using Orchard.Core.Common.Models;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Users.Models;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Xml.Linq;

    public class TicketDriver : ContentPartDriver<TicketPart>
    {
        private readonly IBasicDataService basicDataService;
        protected readonly ICRMContentOwnershipService contentOwnershipService;
        protected readonly IRepository<TicketIdentityRecord> ticketIdentityRecordRepository;
        protected IOrchardServices orchardServices;
        public Localizer T { get; set; }

        public const string EmptyString = "-";

        public TicketDriver(
            IBasicDataService basicDataService,
            ICRMContentOwnershipService contentOwnershipService,
            IOrchardServices orchardServices,
            IRepository<TicketIdentityRecord> ticketIdentityRecordRepository)
        {
            this.basicDataService = basicDataService;
            this.ticketIdentityRecordRepository = ticketIdentityRecordRepository;
            this.orchardServices = orchardServices;
            this.contentOwnershipService = contentOwnershipService;
            this.T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(TicketPart part, string displayType, dynamic shapeHelper)
        {
            if (!this.contentOwnershipService.CurrentUserCanViewContent(part))
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to view the item"));
            }

            var priorities = this.basicDataService.GetPriorities().ToList();
            var statusRecords = this.basicDataService.GetStatusRecords()
                .Select(c => new StatusRecordViewModel { Id = c.Id, Name = c.Name, OrderId = c.OrderId, StatusTypeId = c.StatusTypeId })
                .Cast<BasicDataRecordViewModel>()
                .ToList();
            var serviceRecords = this.basicDataService.GetServices().ToList();
            var ticketTypes = this.basicDataService.GetTicketTypes().ToList();

            TicketViewModel model = this.Convert(part.Record, serviceRecords, priorities, statusRecords, ticketTypes);

            // Fill Priorities
            Converter.Fill(model.Priorities, model.PriorityId, priorities);

            // Fill ticketTypes
            Converter.Fill(model.Types, model.TypeId, ticketTypes);

            // Fill Status Records
            Converter.Fill(model.StatusItems, model.StatusId, statusRecords);

            // Fill Service Records
            Converter.Fill(model.Services, model.ServiceId, serviceRecords);

            model.CurrentUserCanEditItem = this.contentOwnershipService.CurrentUserCanEditContent(part.ContentItem);
            model.CurrentUserIsCustomer = this.contentOwnershipService.IsCurrentUserCustomer();
            model.CurrentUserCanChangePermission = this.contentOwnershipService.CurrentUserCanChangePermission(part.ContentItem);

            switch (displayType)
            {
                case "GanttChart":
                    this.FillStatusTimes(model, part, statusRecords.Cast<StatusRecordViewModel>());
                    return ContentShape("Parts_Ticket_GanttChart",
                        () => shapeHelper.Parts_Ticket_GanttChart(
                            Model: model));

                case "TableRow":
                    return ContentShape("Parts_Ticket_TableRow",
                        () => shapeHelper.Parts_Ticket_TableRow(
                            Model: model));

                case "SummaryAdmin":
                    return ContentShape("Parts_Ticket_SummaryAdmin",
                        () => shapeHelper.Parts_Ticket_SummaryAdmin(
                            Model: model));

                case "Summary":
                    return ContentShape("Parts_Ticket_Summary",
                        () => shapeHelper.Parts_Ticket_Summary(
                            Model: model));

                case "Pinboard":
                    return ContentShape("Parts_Ticket_Pinboard",
                        () => shapeHelper.Parts_Ticket_Pinboard(
                            Model: model));

                case "TitleOnly":
                    return ContentShape("Parts_Ticket_TitleOnly",
                        () => shapeHelper.Parts_Ticket_TitleOnly(
                            Model: model));

                default:

                    // Sub tickets. Customer users can not see the sub-tickets
                    if (!this.contentOwnershipService.IsCurrentUserCustomer())
                    {
                        foreach (var subTicket in part.Record.SubTickets)
                        {
                            model.SubTickets.Add(this.Convert(subTicket, serviceRecords, priorities, statusRecords, ticketTypes));
                        }
                    }

                    // get requesting user
                    if (part.Record.RequestingUser != null)
                    {
                        var userContentItem = this.orchardServices.ContentManager.Get(part.Record.RequestingUser.Id);
                        var userPart = userContentItem.As<UserPart>();

                        model.RequestingUserFullName = CRMHelper.GetFullNameOfUser(userPart);
                        model.RequestingUserEmail = userPart.Email;
                        model.RequestingUsername = userPart.UserName;
                    }

                    List<DriverResult> shapes = new List<DriverResult>();
                    shapes.Add(ContentShape("Parts_Ticket", () => shapeHelper.Parts_Ticket(Model: model)));

                    shapes.Add(ContentShape("Parts_Ticket_HeaderTitle", () => shapeHelper.Parts_Ticket_HeaderTitle(
                        Model: model,
                        Title: model.Title,
                        TicketId: model.ContentItemId,
                        TicketNumber: model.TicketNumber,
                        ParentTicketNumber: model.ParentTicketNumber,
                        ParentTitle: model.ParentTicketTitle,
                        ParentTicketId: model.ParentTicketId)));

                    if (part.Record.RelatedContentItem != null)
                    {
                        shapes.Add(this.RenderRelatedContentItem(part, shapeHelper, "Parts_Ticket_RelatedContentItem"));
                    }

                    shapes.Add(this.ContentShape("Parts_Ticket_Properties", () => shapeHelper.Parts_Ticket_Properties(Model: model)));
                    shapes.Add(this.ContentShape("Parts_Ticket_SubTickets", () => shapeHelper.Parts_Ticket_SubTickets(Model: model)));
                    shapes.Add(this.ContentShape("Parts_Ticket_Dates", () => shapeHelper.Parts_Ticket_Dates(Model: model)));
                    shapes.Add(this.ContentShape("Parts_Ticket_People", () => shapeHelper.Parts_Ticket_People(Model: model)));

                    return Combined(shapes.ToArray());
            }
        }

        protected void FillStatusTimes(TicketViewModel model, TicketPart part, IEnumerable<StatusRecordViewModel> statusRecords)
        {
            CommonPart commonPart =part.As<CommonPart>();
            List<KeyValuePair<int, DateTime>> output = new List<KeyValuePair<int,DateTime>>();

            // creation time
            if (commonPart != null && commonPart.CreatedUtc.HasValue)
            {
                output.Add(new KeyValuePair<int, DateTime>(StatusRecord.NewStatus, CRMHelper.SetSiteTimeZone(this.orchardServices.WorkContext, commonPart.CreatedUtc.Value)));
            }

            var statusTimes = part.StatusTimes;
            foreach (var item in statusTimes)
            {
                var statusRecord = statusRecords.FirstOrDefault(c => c.Id == item.Key);
                if (statusRecord == null)
                {
                    continue;
                }

                int? statusTypeId = statusRecord.StatusTypeId.HasValue? 
                    statusRecord.StatusTypeId.Value:
                    statusRecords
                    .Where(c=>c.OrderId <= statusRecord.OrderId && c.StatusTypeId.HasValue)
                    .Select(c=>c.StatusTypeId)
                    .OrderByDescending(c=> c)
                    .FirstOrDefault();

                statusTypeId = statusTypeId?? StatusRecord.NewStatus;
                output.Add(new KeyValuePair<int, DateTime>(statusTypeId.Value, CRMHelper.SetSiteTimeZone(this.orchardServices.WorkContext, item.Value)));
            }

            model.StatusTimes.Clear();
            model.StatusTimes.AddRange(output.OrderBy(c => c.Value));
        }

        protected override DriverResult Editor(TicketPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (this.contentOwnershipService.CurrentUserCanEditContent(part.ContentItem))
            {
                EditTicketViewModel model = new EditTicketViewModel();
                updater.TryUpdateModel(model, Prefix, null, null);

                // Requesting User
                if (part.Record.RequestingUser == null)
                {
                    int userId = this.orchardServices.WorkContext.CurrentUser.Id;
                    part.Record.RequestingUser = new UserPartRecord { Id = userId };
                }

                // Title
                part.Record.Title = string.IsNullOrEmpty(model.Title) ? string.Empty : model.Title.Trim();

                // statusId
                part.Record.StatusRecord = model.StatusId.HasValue ? new StatusRecord { Id = model.StatusId.Value } : null;

                // Priority
                part.Record.PriorityRecord = model.PriorityId.HasValue ? new PriorityRecord { Id = model.PriorityId.Value } : null;

                // TicketType
                part.Record.TicketType = model.TypeId.HasValue ? new TicketTypeRecord { Id = model.TypeId.Value } : null;

                // Service
                part.Record.Service = model.ServiceId.HasValue ? new ServiceRecord { Id = model.ServiceId.Value } : null;

                // DueDate
                part.Record.DueDate = model.DueDate;

                // Description
                part.Description = model.Text;

                // RelatedContentItemId
                if (model.RelatedContentItemId.HasValue)
                {
                    part.Record.RelatedContentItem = new ContentItemRecord { Id = model.RelatedContentItemId.Value };
                }

                // Parent TicketId
                if (model.ParentTicketId.HasValue)
                {
                    var parentTicket = this.orchardServices.ContentManager.Get(model.ParentTicketId.Value);
                    if (parentTicket.ContentType == "Ticket" && this.contentOwnershipService.CurrentUserCanViewContent(parentTicket))
                    {
                        part.Record.Parent = new TicketPartRecord { Id = model.ParentTicketId.Value };
                    }
                }

                if (part.Record.Identity == null)
                {
                    TicketIdentityRecord identity = new TicketIdentityRecord();
                    this.ticketIdentityRecordRepository.Create(identity);
                    part.Record.Identity = identity;
                }

                return Editor(part, shapeHelper);
            }
            else
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to view the item"));
            }
        }

        protected override DriverResult Editor(TicketPart part, dynamic shapeHelper)
        {
            var priorities = this.basicDataService.GetPriorities().ToList();
            var serviceRecords = this.basicDataService.GetServices().ToList();
            var ticketTypes = this.basicDataService.GetTicketTypes().ToList();
            var statusRecords = this.basicDataService.GetStatusRecords().ToList().Select(c => new BasicDataRecordViewModel { Id = c.Id, Name = c.Name }).ToList();

            // It is worth to mention that the Ticket.Parent in the add mode is set by the controller, before running the drivers

            TicketViewModel model = this.Convert(part.Record, serviceRecords, priorities, statusRecords, ticketTypes);

            // Fill ticketTypes
            Converter.Fill(model.Types, model.TypeId, ticketTypes);
            model.Types.Insert(0, new SelectListItem());

            // Fill Priorities
            Converter.Fill(model.Priorities, model.PriorityId, priorities);
            model.Priorities.Insert(0, new SelectListItem());

            // Fill Service Records
            Converter.Fill(model.Services, model.ServiceId, serviceRecords);
            model.Services.Insert(0, new SelectListItem());

            // Fill Status Records
            Converter.Fill(model.StatusItems, model.StatusId, statusRecords);
            model.StatusItems.Insert(0, new SelectListItem());

            model.CurrentUserCanEditItem = this.contentOwnershipService.CurrentUserCanEditContent(part.ContentItem);

            if (model.CurrentUserCanEditItem || part.Id == default(int) || this.contentOwnershipService.CurrentUserCanViewContent(part.ContentItem))
            {
                List<ContentShapeResult> shapes = new List<ContentShapeResult>();
                shapes.Add(ContentShape("Parts_Ticket_EditTitle", () => shapeHelper.Parts_Ticket_EditTitle(
                        TicketId: part.Id,
                        Title: model.Title,
                        TicketNumber: model.TicketNumber,
                        ParentTitle: model.ParentTicketTitle,
                        ParentTicketId: model.ParentTicketId,
                        ParentTicketNumber: model.ParentTicketNumber)));

                bool isCustomer = this.contentOwnershipService.IsCurrentUserCustomer();
                model.CurrentUserIsCustomer = isCustomer;

                shapes.Add(ContentShape("Parts_Ticket_Edit",
                        () => shapeHelper.EditorTemplate(
                            TemplateName: "Parts/Ticket",
                            Model: model,
                            Prefix: Prefix)));

                shapes.Add(ContentShape("Parts_Ticket_Properties_Edit",
                      () => shapeHelper.EditorTemplate(
                          TemplateName: "Parts/TicketProperties",
                          Model: model,
                          Prefix: Prefix)));

                if (!isCustomer)
                {
                    shapes.Add(ContentShape("Parts_Ticket_Dates_Edit",
                          () => shapeHelper.EditorTemplate(
                              TemplateName: "Parts/TicketDates",
                              Model: model,
                              Prefix: Prefix)));

                    shapes.Add(ContentShape("Parts_Ticket_People_Edit",
                          () => shapeHelper.EditorTemplate(
                              TemplateName: "Parts/TicketPeople",
                              Model: model,
                              Prefix: Prefix)));

                }

                if (part.Record.RelatedContentItem != null)
                {
                    shapes.Add(this.RenderRelatedContentItem(part, shapeHelper, "Parts_Ticket_RelatedContentItem_Edit"));
                }

                return Combined(shapes.ToArray());
            }
            else
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to view the item"));
            }
        }

        protected ContentShapeResult RenderRelatedContentItem(TicketPart part, dynamic shapeHelper, string shapeType)
        {
            var relatedContentItem = this.orchardServices.ContentManager.Get(part.Record.RelatedContentItem.Id, VersionOptions.AllVersions);
            var latestVersion = relatedContentItem.Record.Versions.OrderByDescending(c => c.Id).FirstOrDefault();
            bool isPublished = latestVersion != null ? latestVersion.Published : false;
            dynamic model = new ExpandoObject();
            model.ContentItem = relatedContentItem;
            model.IsPublished = isPublished;
            return ContentShape(shapeType, () => shapeHelper.Parts_Ticket_RelatedContentItem(Model: model));

        }

        protected override void Exporting(TicketPart part, ExportContentContext context)
        {
            var mainElement = context.Element(part.PartDefinition.Name);

            Func<IBasicDataRecord, int?> getValue = (record) => record != null ? (int?)record.Id : null;

            mainElement.SetAttributeValue(TicketPart.DueDateFieldName, part.Record.DueDate);
            mainElement.SetAttributeValue(TicketPart.PriorityFieldName, getValue(part.Record.PriorityRecord));
            mainElement.SetAttributeValue(TicketPart.ServiceFieldName, getValue(part.Record.Service));
            mainElement.SetAttributeValue(TicketPart.TypeFieldName, getValue(part.Record.TicketType));
            mainElement.SetAttributeValue(TicketPart.TitleFieldName, part.Record.Title);
            mainElement.SetAttributeValue(TicketPart.StatusFieldName, part.Record.StatusRecord != null ? (int?)part.Record.StatusRecord.Id : null);

            var descriptionElement = mainElement.El("Description", new XElement("Description", part.Record.Description));
        }

        protected override void Importing(TicketPart part, ImportContentContext context)
        {
            // due date
            var dueDate = context.Attribute(part.PartDefinition.Name, TicketPart.DueDateFieldName);
            part.Record.DueDate = string.IsNullOrEmpty(dueDate) ? null : (DateTime?)DateTime.Parse(dueDate);

            // service
            var serviceId = context.Attribute(part.PartDefinition.Name, TicketPart.ServiceFieldName);
            part.Record.Service = string.IsNullOrEmpty(serviceId) ? null : new ServiceRecord { Id = int.Parse(serviceId) };

            // priority
            var priorityId = context.Attribute(part.PartDefinition.Name, TicketPart.PriorityFieldName);
            part.Record.PriorityRecord = string.IsNullOrEmpty(priorityId) ? null : new PriorityRecord { Id = int.Parse(priorityId) };

            // type
            var typeId = context.Attribute(part.PartDefinition.Name, TicketPart.TypeFieldName);
            part.Record.TicketType = string.IsNullOrEmpty(typeId) ? null : new TicketTypeRecord { Id = int.Parse(typeId) };

            // Title
            part.Record.Title = context.Attribute(part.PartDefinition.Name, TicketPart.TitleFieldName);

            // Description
            var mainElement = context.Data.Element(part.PartDefinition.Name);
            part.Record.Description = mainElement.El(TicketPart.DescriptionFieldName);
        }

        private TicketViewModel Convert(TicketPartRecord record, List<ServiceRecord> serviceRecords, List<PriorityRecord> priorities, List<BasicDataRecordViewModel> statusRecords, List<TicketTypeRecord> ticketTypes)
        {
            TicketViewModel model = new TicketViewModel
              {
                  TicketId = record.Id,
                  StatusId = record.StatusRecord != null ? (int?)record.StatusRecord.Id : null,
                  PriorityId = record.PriorityRecord != null ? (int?)record.PriorityRecord.Id : null,
                  ServiceId = record.Service != null ? (int?)record.Service.Id : null,
                  TypeId = record.TicketType != null ? (int?)record.TicketType.Id : null,
                  ParentTicketId = record.Parent != null ? (int?)record.Parent.Id : null,
                  ParentTicketTitle = record.Parent != null ? record.Parent.Title : string.Empty,
                  RelatedContentItemId = record.RelatedContentItem != null ? (int?)record.RelatedContentItem.Id : null,
                  Text = record.Description,
                  TicketNumber = record.Identity != null ? (int)record.Identity.Id : default(int),
                  Title = record.Title,
                  DueDate = record.DueDate,
                  SourceId = record.SourceId,
                  SourceData = record.SourceData
              };

            if (record.ContentItemRecord != null)
            {
                model.ContentItemId = record.ContentItemRecord.Id;
            }

            if (record.Parent != null && record.Parent.Identity != null)
            {
                model.ParentTicketNumber = record.Parent.Identity.Id;
            }

            // set status name
            model.StatusName = this.GetBasicDataRecordName(model.StatusId, statusRecords);

            // Priority name
            model.PriorityName = this.GetBasicDataRecordName(model.PriorityId, priorities);

            // Service Name
            model.ServiceName = this.GetBasicDataRecordName(model.ServiceId, serviceRecords);

            // Ticket Type name
            model.TypeName = this.GetBasicDataRecordName(model.TypeId, ticketTypes);

            return model;
        }

        private string GetBasicDataRecordName<T>(int? id, IList<T> items)
            where T : IBasicDataRecord
        {
            if (id == null)
            {
                return this.T(EmptyString).ToString();
            }

            var record = items.FirstOrDefault(c => c.Id == id);

            if (record == null)
            {
                return this.T(EmptyString).ToString();
            }

            return record.Name;
        }
    }
}