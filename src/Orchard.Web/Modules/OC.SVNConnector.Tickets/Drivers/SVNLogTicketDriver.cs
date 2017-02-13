using OC.SVNConnector.Models;
using OC.SVNConnector.Tickets.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace OC.SVNConnector.Tickets.Drivers
{
    public class SVNLogTicketDriver : ContentPartDriver<SVNLogsTicketPart>
    {
        private readonly IOrchardServices services;
        public SVNLogTicketDriver(IOrchardServices services)
        {
            this.services = services;
        }

        protected override DriverResult Display(SVNLogsTicketPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType != "Detail")
            {
                return null;
            }

            var ticketPart = part.As<TicketPart>();

            if (ticketPart == null || ticketPart.Record.Identity == null || string.IsNullOrEmpty(part.SVNLogs))
            {
                return null;
            }

            var svnLogIds = part.SVNLogs.Split(',').Select(c => int.Parse(c));
            var logs = this.services.ContentManager.GetMany<SVNLogPart>(svnLogIds, VersionOptions.Published, QueryHints.Empty);

            var listShape = shapeHelper.List();
            listShape.AddRange(logs.Select(item => services.ContentManager.BuildDisplay(item, "Detail")));
            listShape.Classes.Add("content-items");
            listShape.Classes.Add("list-items");

            var result = new List<DriverResult>();

            result.Add(ContentShape("Parts_SVNLogsTicket", () => shapeHelper.Parts_SVNLogsTicket(Model: listShape)));

            dynamic headerModel = new ExpandoObject();
            headerModel.Count = logs.Count();
            result.Add(ContentShape("Parts_SVNLogsTicket_Header", () => shapeHelper.Parts_SVNLogsTicket_Header(Model: headerModel)));

            return Combined(result.ToArray());
        }
    }
}