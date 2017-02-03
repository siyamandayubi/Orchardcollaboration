using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Project.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.CRM.Project.Handlers
{
    public class AttachToMilestoneHandler : ContentHandler
    {
        public AttachToMilestoneHandler(IRepository<AttachToMilestonePartRecord> repository, IContentManager contentManager)
        {
            Filters.Add(StorageFilter.For(repository));

            this.OnPublishing<AttachToMilestonePart>((context, part) =>
            {
                // refresh the milestone of the child items
                TicketPart ticketPart = part.As<TicketPart>();
                if (ticketPart != null && ticketPart.Record.SubTickets != null && ticketPart.Record.SubTickets.Count > 0)
                {
                    var childTickets = contentManager.GetMany<AttachToMilestonePart>(
                        ticketPart.Record.SubTickets.Select(c => c.Id), 
                        VersionOptions.Published, 
                        new QueryHints().ExpandParts<AttachToMilestonePart>());

                    foreach (var childTicket in childTickets)
                    {
                        childTicket.Record.MilestoneId = part.Record.MilestoneId;
                        contentManager.Publish(childTicket.ContentItem);
                    }
                }
            });
        }
    }
}