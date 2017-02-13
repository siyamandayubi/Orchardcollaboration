using Orchard.CRM.Core.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Services;

namespace Orchard.CRM.Core.Activities
{
    public class TicketTypeBranchActivity : BasicDataBranchActivity<TicketTypeRecord>
    {
        public override string ActivityName { get { return "TicketTypeBranch"; } }
        public override string UnknownValue { get { return "UnknownTicketType"; } }
        public override string BasicDataRecordName { get { return "Ticket Type"; } }
        private readonly IBasicDataService basicDataService;

        public TicketTypeBranchActivity(
                IBasicDataService basicDataService,
                IContentManager contentManager)
            : base(contentManager)
        {
            this.basicDataService = basicDataService;
        }

        protected override TicketTypeRecord GetFromTicket(TicketPart ticketPart)
        {
            var record = ticketPart.Record.TicketType;
            if (record == null)
            {
                return null;
            }
            else
            {
                var records = this.basicDataService.GetTicketTypes().ToList();
                return records.FirstOrDefault(c => c.Id == record.Id);
            }
        }

        protected override IEnumerable<TicketTypeRecord> GetData()
        {
            return this.basicDataService.GetTicketTypes();
        }
    }
}