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
    public class ServiceBranchActivity : BasicDataBranchActivity<ServiceRecord>
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

        protected override ServiceRecord GetFromTicket(TicketPart ticketPart)
        {
            var record = ticketPart.Record.Service;
            if (record == null)
            {
                return null;
            }
            else
            {
                var records = this.basicDataService.GetServices().ToList();
                return records.FirstOrDefault(c => c.Id == record.Id);
            }
        }

        protected override IEnumerable<ServiceRecord> GetData()
        {
            return this.basicDataService.GetServices();
        }
    }
}