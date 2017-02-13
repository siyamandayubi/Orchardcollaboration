using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.Localization;
using System.Globalization;

namespace Orchard.CRM.Core.Providers.ActivityStream.Descriptors
{
    public class TicketDescriptor : IContentItemDescriptor
    {
        public TicketDescriptor()
        {
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string GetDescription(IContent content)
        {
            var ticket = content.As<TicketPart>();
            if (ticket == null)
            {
                return string.Empty;
            }

            // Ticket without identity means it is a new ticket
            if (ticket.Record.Identity != null)
            {
                return T("{0} - {1}", ticket.Record.Identity.Id.ToString(CultureInfo.InvariantCulture), ticket.Record.Title).Text;
            }
            else
            {
                return T("New Ticket").Text;
            }
        }

        public bool CanApply(IContent content)
        {
            return content.As<TicketPart>() != null;
        }
    }
}