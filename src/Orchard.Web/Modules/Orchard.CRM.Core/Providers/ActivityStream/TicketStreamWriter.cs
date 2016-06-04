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

namespace Orchard.CRM.Core.Providers.ActivityStream
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.Data;
    using Orchard.Localization;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Routing;

    public class TicketStreamWriter : IActivityStreamWriter
    {
        private readonly IOrchardServices services;
        private readonly IBasicDataService basicDataService;
        private readonly IRepository<TicketPartRecord> ticketRepository;

        public TicketStreamWriter(
            IOrchardServices services,
            IBasicDataService basicDataService,
            IRepository<TicketPartRecord> ticketRepository)
        {
            this.ticketRepository = ticketRepository;
            this.basicDataService = basicDataService;
            this.services = services;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            return context.ContentItem.As<TicketPart>() != null;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            List<string> changes = new List<string>();

            TicketPartRecord old = context.Snapshot != null ? (context.Snapshot as TicketPartRecord) : null;
            TicketPartRecord newValue = (context.ContentItem.As<TicketPart>()).Record;

            if (old == null)
            {
                string change = T("Ticket is created").Text;
                return new[] { new ActivityStreamChangeItem(change) };
            }

            // change status
            this.AddBasicDataRecordChange(
                changes,
                old.StatusRecord,
                newValue.StatusRecord,
                (id) => this.basicDataService.GetStatusRecords().FirstOrDefault(c => c.Id == id),
                "changed the status to: '{0}'");

            // change ticketType
            this.AddBasicDataRecordChange(
                changes,
                old.TicketType,
                newValue.TicketType,
                (id) => this.basicDataService.GetTicketTypes().FirstOrDefault(c => c.Id == id),
                "changed the Ticket Type to: '{0}'");

            // change service
            this.AddBasicDataRecordChange(
                changes,
                old.Service,
                newValue.Service,
                (id) => this.basicDataService.GetServices().FirstOrDefault(c => c.Id == id),
                "changed the Service to: '{0}'");

            // change priority
            this.AddBasicDataRecordChange(
                changes,
                old.PriorityRecord,
                newValue.PriorityRecord,
                (id) => this.basicDataService.GetPriorities().FirstOrDefault(c => c.Id == id),
                "changed the Priority to: '{0}'");

            // change DueDate
            if (old.DueDate != newValue.DueDate)
            {
                string newDueDateString = newValue.DueDate != null ? T(newValue.DueDate.Value.ToLongDateString() + " " + newValue.DueDate.Value.ToLongTimeString()).Text : this.T("null").Text;
                newDueDateString = string.Format(
                    CultureInfo.CurrentUICulture,
                    T("changed the DueDate to: '{0}'").Text,
                    newDueDateString);

                changes.Add(newDueDateString);
            }

            // change Title
            if (old.Title != newValue.Title)
            {
                string newTitleString = !string.IsNullOrEmpty(newValue.Title) ? newValue.Title : this.T("Empty").Text;
                newTitleString = string.Format(
                    CultureInfo.CurrentUICulture,
                    T("changed the Title to: '{0}'").Text,
                    newTitleString);

                changes.Add(newTitleString);
            }

            // change Description
            if (old.Description != newValue.Description)
            {
                string newDescriptionString = !string.IsNullOrEmpty(newValue.Description) ? newValue.Description : this.T("Empty").Text;
                newDescriptionString = T("changed the Description").Text;

                changes.Add(newDescriptionString);
            }

            return changes.Select(c => new ActivityStreamChangeItem(c));
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            var ticketPart = context.ContentItem.As<TicketPart>();

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("action", "Display");
            routeValueDictionary.Add("controller", "Ticket");
            routeValueDictionary.Add("area", "Orchard.CRM.Core");
            routeValueDictionary.Add("id", context.ContentItem.Id);
            ActivityStreamContentDescription returnValue = new ActivityStreamContentDescription(StreamWriters.TicketStreamWriter) { Weight = 1, RouteValues = routeValueDictionary };

            // new ticket
            if (context.Snapshot == null)
            {
                returnValue.Description = this.GetCreatedTicketDescription(ticketPart);
                return returnValue;
            }

            string ticketDescription = this.GetTicketIdAndTitle(ticketPart.Record);

            returnValue.Description = string.Format("{0} {1}", this.T("Changed Ticket").Text, ticketDescription);
            return returnValue;
        }

        private void AddBasicDataRecordChange(List<string> changes, IBasicDataRecord oldRecord, IBasicDataRecord newRecord, Func<int, IBasicDataRecord> getRecord, string format)
        {
            if (oldRecord == null && newRecord != null ||
                (oldRecord != null && newRecord != null && oldRecord.Id != newRecord.Id))
            {
                var newValue = getRecord(newRecord.Id);
                string name = newValue != null ? T(newValue.Name).Text : newValue.Id.ToString(CultureInfo.InvariantCulture);

                changes.Add(string.Format(CultureInfo.CurrentUICulture, T(format).Text, name));
            }
            else if (oldRecord != null && newRecord == null)
            {
                changes.Add(string.Format(CultureInfo.CurrentUICulture, T(format).Text, T("[NULL]").Text));
            }
        }

        private string GetTicketIdAndTitle(TicketPartRecord ticektPart)
        {
            string identityString = ticektPart.Identity != null ?
                ticektPart.Identity.Id.ToString(CultureInfo.InvariantCulture) :
                string.Empty;

            string title = ticektPart.Title;

            return string.Format("{0} - {1}", identityString, title);
        }

        private string GetCreatedTicketDescription(TicketPart ticket)
        {
            string ticketIdAndTitle = this.GetTicketIdAndTitle(ticket.Record);
            string createTicketDescription = string.Format(
                "{0} '{1}'",
                T("Creates a new Ticket").Text,
                ticketIdAndTitle);

            if (ticket.Record.Parent != null)
            {
                var parentTicket = this.ticketRepository.Table.FirstOrDefault(c => c.Id == ticket.Record.Parent.Id);
                if (parentTicket != null)
                {
                    createTicketDescription = string.Format(
                        "Creates a sub ticket '{0}' for '{1}'",
                        this.GetTicketIdAndTitle(parentTicket),
                        ticketIdAndTitle);
                }
            }

            string relatedContentItemString = string.Empty;
            if (ticket.Record.RelatedContentItem != null)
            {
                var relatedContentItem = this.services.ContentManager.Get(ticket.Record.RelatedContentItem.Id);
                relatedContentItemString = string.Format(" {0} {1}", this.T("for").Text, CRMHelper.GetContentItemTitle(relatedContentItem));
            }

            return string.Format("{0}{1}", createTicketDescription, relatedContentItemString);
        }

        public string Name
        {
            get { return "Ticket"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            var part = contentItem.As<TicketPart>();

            if (part == null)
            {
                return null;
            }

            TicketPartRecord oldData = new TicketPartRecord();
            oldData.StatusRecord = part.Record.StatusRecord;
            oldData.TicketType = part.Record.TicketType;
            oldData.Title = part.Record.Title;
            oldData.Description = part.Record.Description;
            oldData.DueDate = part.Record.DueDate;
            oldData.PriorityRecord = part.Record.PriorityRecord;
            oldData.Service = part.Record.Service;

            return oldData;
        }
    }
}