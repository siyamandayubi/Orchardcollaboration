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

using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.Data;
using System.Web.Routing;
using System;
using System.Globalization;

namespace Orchard.CRM.Core.Handlers
{
    public class TicketMenuItemHandler : ContentHandler
    {
        public TicketMenuItemHandler(IRepository<TicketMenuItemPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));

            this.OnRemoved<TicketMenuItemPart>((contextPart, part) =>
            {
                repository.Delete(part.Record);
            });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            base.GetItemMetadata(context);

            var ticketItemMenu = context.ContentItem.As<TicketMenuItemPart>();

            if (ticketItemMenu != null)
            {
                var routeData = new RouteValueDictionary();

                routeData["controller"] = "Ticket";
                routeData["action"] = "Search";
                routeData["area"] = "Orchard.CRM.Core";

                var record = ticketItemMenu.Record;

                if (record.DueDateDays != null)
                {
                    string maxDueDate = DateTime.UtcNow.AddDays(record.DueDateDays.Value).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                    routeData["DueDate"] = maxDueDate;
                }

                if (record.BusinessUnit != null)
                {
                    routeData["BusinessUnits"] = record.BusinessUnit.Id;
                }

                if (record.User != null)
                {
                    routeData["Users"] = record.User.Id;
                }

                if (record.Status != null)
                {
                    routeData["Status"] = record.Status.Id;
                }

                context.Metadata.DisplayRouteValues = routeData;
            }
        }
    }
}