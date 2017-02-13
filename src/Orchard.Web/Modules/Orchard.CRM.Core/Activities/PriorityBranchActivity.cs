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
    public class PriorityBranchActivity : BasicDataBranchActivity<PriorityRecord>
    {
        public override string ActivityName { get { return "PriorityBranch"; } }
        public override string UnknownValue { get { return "UnknownPriority"; } }
        public override string BasicDataRecordName { get { return "Priority"; } }
        private readonly IBasicDataService basicDataService;

        public PriorityBranchActivity(
                IBasicDataService basicDataService,
                IContentManager contentManager)
            : base(contentManager)
        {
            this.basicDataService = basicDataService;
        }

        protected override PriorityRecord GetFromTicket(TicketPart ticketPart)
        {
            var record = ticketPart.Record.PriorityRecord;
            if (record == null)
            {
                return null;
            }
            else
            {
                var records = this.basicDataService.GetPriorities().ToList();
                return records.FirstOrDefault(c => c.Id == record.Id);
            }
        }

        protected override IEnumerable<PriorityRecord> GetData()
        {
            return this.basicDataService.GetPriorities().ToList();
        }
    }
}