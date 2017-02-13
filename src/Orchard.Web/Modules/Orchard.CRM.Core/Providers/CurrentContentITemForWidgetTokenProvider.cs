using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers
{
    public class CurrentContentItemForWidgetTokenProvider : ContentHandler, ITokenProvider, ICurrentContentItemForWidgetTokenProvider
    {
        private Localizer T { get; set; }
        private BusinessUnitPart businessUnitPart;
        private TeamPart teamPart;
        private TicketPart ticketPart;
        private ContentItem contentItem;
        private List<ContentItemPermissionPart> permissionParts = new List<ContentItemPermissionPart>();

        public CurrentContentItemForWidgetTokenProvider()
        {
            T = NullLocalizer.Instance;
        }

        protected override void BuildDisplayShape(BuildDisplayContext context)
        {
            if (context.DisplayType == "Detail")
            {
                this.SetParts(context);
            }

            if (this.contentItem == null)
            {
                this.contentItem = context.ContentItem;
            }
        }

        protected override void BuildEditorShape(BuildEditorContext context)
        {
            this.SetParts(context);
        }

        public void Describe(DescribeContext context)
        {
            context.For("WContentItem", T("Main ContentItem"), T("Main ContentItem to use in widgets"))
                .Token("Id", T("Id"), T("ContentItem Id"));

            context.For("Team", T("Team"), T("The current Team"))
                .Token("Id", T("Id"), T("The Team Id"));

            context.For("Ticket", T("Ticket"), T("The current Ticket"))
                .Token("Id", T("Id"), T("The Ticket Id"));

            context.For("BusinessUnit", T("BusinessUnit"), T("The current BusinessUnit"))
                .Token("Id", T("Id"), T("The BusinessUnit Id"));
        }

        public void Evaluate(EvaluateContext context)
        {
            if (this.teamPart != null)
            {
                context.For("Team", () => this.teamPart).Token("Id", c => c.Id);
            }

            if (this.ticketPart != null)
            {
                context.For("Ticket", () => this.ticketPart).Token("Id", c => c.Id);
            }

            if (this.businessUnitPart != null)
            {
                context.For("BusinessUnit", () => this.businessUnitPart).Token("Id", c => c.Id);
            }

            if (this.contentItem != null)
            {
                context.For("WContentItem", () => this.contentItem).Token("Id", c => c.Id);
            }
        }

        private void SetParts(BuildShapeContext context)
        {
            var tempPartTemp = context.ContentItem.As<TeamPart>();
            if (tempPartTemp != null)
            {
                this.teamPart = tempPartTemp;
            }

            var tempBusinessUnitPart = context.ContentItem.As<BusinessUnitPart>();
            if (tempBusinessUnitPart != null)
            {
                this.businessUnitPart = tempBusinessUnitPart;
            }

            var tempTicketPart = context.ContentItem.As<TicketPart>();
            if (tempTicketPart != null)
            {
                this.ticketPart = tempTicketPart;
            }

            if (this.contentItem == null)
            {
                this.contentItem = context.ContentItem;
            }

            var contentItemPermissionPart = context.ContentItem.As<ContentItemPermissionPart>();
            if (contentItemPermissionPart != null)
            {
                this.permissionParts.Add(contentItemPermissionPart);
            }
        }

        public IEnumerable<ContentItemPermissionPart> GetCurrentItemsPermissionParts()
        {
            return this.permissionParts;
        }
    }
}