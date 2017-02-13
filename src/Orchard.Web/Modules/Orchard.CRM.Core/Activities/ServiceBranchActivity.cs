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
    public class ServiceBranchActivity : BasicDataBranchActivity<ServicePartRecord>
    {
        public override string ActivityName { get { return "ServiceBranch"; } }
        public override string UnknownValue { get { return "UnknownService"; } }
        public override string BasicDataRecordName { get { return "Service"; } }
        private readonly IBasicDataService basicDataService;

        public ServiceBranchActivity(
                IBasicDataService basicDataService,
                IContentManager contentManager)
            : base(contentManager)
        {
            this.basicDataService = basicDataService;
        }

        protected override ServicePartRecord GetFromTicket(TicketPart ticketPart)
        {
            var record = ticketPart.Record.Service;
            if (record == null)
            {
                return null;
            }
            else
            {
                var records = this.basicDataService.GetServices().ToList().Select(c => c.Record);
                return records.FirstOrDefault(c => c.Id == record.Id);
            }
        }

        protected override IEnumerable<ServicePartRecord> GetData()
        {
            return this.basicDataService.GetServices().Select(c => c.Record);
        }
    }
}