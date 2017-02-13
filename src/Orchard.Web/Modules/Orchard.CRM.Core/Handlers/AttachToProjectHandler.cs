using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Handlers
{
    public class AttachToProjectHandler : ContentHandler
    {
        public AttachToProjectHandler(IRepository<AttachToProjectPartRecord> repository, IContentManager contentManager)
        {
            Filters.Add(StorageFilter.For(repository));

            this.OnIndexing<AttachToProjectPart>((context, part) =>
            {
                string projectId = part.Record.Project != null ? part.Record.Project.Id.ToString(CultureInfo.InvariantCulture) : TicketPart.NullValueForIntegers;
                context.DocumentIndex
                    .Add(AttachToProjectPart.ProjectIdFieldName, projectId).Store();

                context.DocumentIndex.Add("type", part.ContentItem.ContentType.ToLower(CultureInfo.InvariantCulture));
            });

            this.OnPublishing<AttachToProjectPart>((context, part) =>
            {
                // refresh the AttachToProjectPart of the child items
                TicketPart ticketPart = part.As<TicketPart>();
                if (ticketPart != null && ticketPart.Record.SubTickets != null && ticketPart.Record.SubTickets.Count > 0)
                {
                    var childTickets = contentManager.GetMany<AttachToProjectPart>(
                        ticketPart.Record.SubTickets.Select(c => c.Id),
                        VersionOptions.Published,
                        new QueryHints().ExpandParts<AttachToProjectPart>());

                    foreach (var childTicket in childTickets)
                    {
                        childTicket.Record.Project = part.Record.Project != null ? new ProjectPartRecord { Id = part.Record.Project.Id } : null;
                        contentManager.Publish(childTicket.ContentItem);
                    }
                }
            });

        }
    }
}