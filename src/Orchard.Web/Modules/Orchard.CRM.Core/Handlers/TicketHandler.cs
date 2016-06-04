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

namespace Orchard.CRM.Core.Handlers
{
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Handlers;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.Data;
    using Orchard.Localization;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;

    public class TicketHandler : ContentHandler
    {
        private readonly IBasicDataService basicDataService;
        private readonly IActivityStreamService activityStreamService;
        private readonly IRepository<CRMCommentPartRecord> commentRepository;
        private readonly ISessionLocator sessionLocator;
        private readonly ITransactionManager transactionManager;
        private readonly IOrchardServices services;
        public Localizer T { get; set; }
        public TicketHandler(
            ITransactionManager transactionManager,
            ISessionLocator sessionLocator,
            IActivityStreamService activityStreamService,
            IRepository<TicketPartRecord> repository,
            IRepository<CRMCommentPartRecord> commentRepository,
            IOrchardServices services,
            IBasicDataService basicDataService)
        {
            this.transactionManager = transactionManager;
            this.sessionLocator = sessionLocator;
            this.services = services;
            this.T = NullLocalizer.Instance;
            this.basicDataService = basicDataService;
            this.commentRepository = commentRepository;
            this.activityStreamService = activityStreamService;

            Filters.Add(StorageFilter.For(repository));

            this.OnPublishing<TicketPart>((context, part) =>
            {
                var statusTimes = part.StatusTimes;

                if (part.Record.StatusRecord == null)
                {
                    statusTimes.Add(new KeyValuePair<int, DateTime>(0, DateTime.UtcNow));    
                }
                else
                {
                    // if the status doesn't change from the last update, then do noting
                    if (statusTimes.Count > 0 && statusTimes[statusTimes.Count - 1].Key == part.Record.StatusRecord.Id)
                    {
                        return;
                    }

                    statusTimes.Add(new KeyValuePair<int, DateTime>(part.Record.StatusRecord.Id, DateTime.UtcNow));
                }

                part.StatusTimes = statusTimes;
            });

            this.OnRemoved<TicketPart>((contextPart, part) =>
            {
                repository.Delete(part.Record);
            });

            this.OnUpdated<TicketPart>((contextPart, part) =>
            {
                // Due to a bug in middle of nowhere in NHibernate, sometimes the due date doesn't persisted properly, so we
                // will update all of the cached copies in the session.
                var session = this.transactionManager.GetSession();
                var sessionContext = session.GetSessionImplementation().PersistenceContext;
                foreach (TicketPartRecord entity in sessionContext.EntitiesByKey.Values.Where(c => c is TicketPartRecord))
                {
                    if (entity.Id == part.Record.Id)
                    {
                        entity.DueDate = part.Record.DueDate;
                    }
                }
            });

            this.OnIndexing<TicketPart>((context, part) =>
            {
                this.UpdateIndex(context, part);
            });
        }

        private void UpdateIndex(IndexContentContext context, TicketPart part)
        {
            Func<IBasicDataRecord, string> getValue = (c) => c != null ? c.Id.ToString(CultureInfo.InvariantCulture) : string.Empty;

            // service
            string service = getValue(part.Record.Service);

            string type = getValue(part.Record.TicketType);

            // identity
            string identity = part.Record.Identity != null ? part.Record.Identity.Id.ToString(CultureInfo.InvariantCulture) : TicketPart.NullValueForIntegers;

            context.DocumentIndex
                .Add(TicketPart.TitleFieldName, part.Record.Title).Analyze().Store()
                .Add(TicketPart.TypeFieldName, type).Store()
                .Add(TicketPart.ServiceFieldName, service).Store()
                .Add(TicketPart.IdentityFieldName, identity).Store()
                .Add("type", "ticket").Store()
                .Add(TicketPart.DescriptionFieldName, part.Record.Description).Analyze().Store();

            // Priority
            if (part.Record.PriorityRecord != null)
            {
                context.DocumentIndex.Add(TicketPart.PriorityFieldName, part.PriorityRecord.Id.ToString(CultureInfo.InvariantCulture)).Store();
                var priorityRecord = basicDataService.GetPriorities().FirstOrDefault(c => c.Id == part.PriorityRecord.Id);

                if (priorityRecord != null)
                {
                    context.DocumentIndex.Add(TicketPart.PriorityOrderFieldName, priorityRecord.OrderId.ToString(CultureInfo.InvariantCulture)).Store();
                }
            }
            else
            {
                context.DocumentIndex.Add(TicketPart.PriorityFieldName, TicketPart.NullValueForIntegers).Store();
                context.DocumentIndex.Add(TicketPart.PriorityOrderFieldName, TicketPart.NullValueForIntegers).Store();
            }

            // status
            if (part.Record.StatusRecord != null)
            {
                context.DocumentIndex.Add(TicketPart.StatusFieldName, part.StatusRecord.Id.ToString(CultureInfo.InvariantCulture)).Store();
                var statusRecord = basicDataService.GetStatusRecords().FirstOrDefault(c => c.Id == part.StatusRecord.Id);

                if (statusRecord != null)
                {
                    context.DocumentIndex.Add(TicketPart.StatusOrderFieldName, statusRecord.OrderId.ToString(CultureInfo.InvariantCulture)).Store();
                }
            }
            else
            {
                context.DocumentIndex.Add(TicketPart.StatusFieldName, TicketPart.NullValueForIntegers).Store();
                context.DocumentIndex.Add(TicketPart.StatusOrderFieldName, TicketPart.NullValueForIntegers).Store();
            }

            // DueDate
            if (part.Record.DueDate != null)
            {
                context.DocumentIndex.Add(TicketPart.DueDateFieldName, part.Record.DueDate.Value).Store();
            }

            // requesting User
            if (part.Record.RequestingUser != null)
            {
                context.DocumentIndex.Add(TicketPart.RequestingUserFieldName, part.Record.RequestingUser.Id.ToString(CultureInfo.InvariantCulture)).Store();
            }
            else
            {
                context.DocumentIndex.Add(TicketPart.RequestingUserFieldName, string.Empty).Store();
            }

            // RelatedContentItem
            if (part.Record.RelatedContentItem != null)
            {
                context.DocumentIndex.Add(TicketPart.RelatedContentItemIdFieldName, part.Record.RelatedContentItem.Id.ToString(CultureInfo.InvariantCulture)).Store();
            }
            else
            {
                context.DocumentIndex.Add(TicketPart.RelatedContentItemIdFieldName, string.Empty).Store();
            }
        }
    }
}