using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OC.SVNConnector.Tickets.Models
{
    public class SVNLogsTicketPart : ContentPart
    {
        /// <summary>
        /// Comma separated list of SVNLogs
        /// </summary>
        public string SVNLogs
        {
            get { return this.Retrieve(x => x.SVNLogs); }
            set { this.Store(x => x.SVNLogs, value); }
        }
    }
}