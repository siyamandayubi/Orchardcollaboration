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