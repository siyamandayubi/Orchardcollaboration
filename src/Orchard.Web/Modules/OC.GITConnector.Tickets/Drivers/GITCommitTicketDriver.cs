using OC.GITConnector.Models;
using OC.GITConnector.Tickets.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace OC.GITConnector.Tickets.Drivers
{
    public class GITCommitTicketDriver : ContentPartDriver<GITCommitsTicketPart>
    {
        private readonly IOrchardServices services;
        public GITCommitTicketDriver(IOrchardServices services)
        {
            this.services = services;
        }

        protected override DriverResult Display(GITCommitsTicketPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType != "Detail")
            {
                return null;
            }

            var ticketPart = part.As<TicketPart>();

            if (ticketPart == null || ticketPart.Record.Identity == null || string.IsNullOrEmpty(part.GITCommits))
            {
                return null;
            }

            var gitLogIds = part.GITCommits.Split(',').Select(c => int.Parse(c));
            var logs = this.services.ContentManager.GetMany<GITCommitPart>(gitLogIds, VersionOptions.Published, QueryHints.Empty);

            var listShape = shapeHelper.List();
            listShape.AddRange(logs.Select(item => services.ContentManager.BuildDisplay(item, "Detail")));
            listShape.Classes.Add("content-items");
            listShape.Classes.Add("list-items");

            var result = new List<DriverResult>();

            result.Add(ContentShape("Parts_GITCommitsTicket", () => shapeHelper.Parts_GITCommitsTicket(Model: listShape)));

            dynamic headerModel = new ExpandoObject();
            headerModel.Count = logs.Count();
            result.Add(ContentShape("Parts_GITCommitsTicket_Header", () => shapeHelper.Parts_GITCommitsTicket_Header(Model: headerModel)));

            return Combined(result.ToArray());
        }
    }
}