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