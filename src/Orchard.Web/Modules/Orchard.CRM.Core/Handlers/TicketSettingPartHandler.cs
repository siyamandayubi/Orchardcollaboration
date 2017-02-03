using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Handlers
{
    public class TicketSettingPartHandler : ContentHandler
    {
        public TicketSettingPartHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<TicketSettingPart>("Site"));
            Filters.Add(new TemplateFilterForPart<TicketSettingPart>("TicketSetting", "Parts/Ticket.TicketSetting", "Orchard Collaboration"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Orchard Collaboration")));
        }
    }
}