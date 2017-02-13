using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OC.GITConnector.Tickets.Models
{
    public class GITCommitsTicketPart : ContentPart
    {
        /// <summary>
        /// Comma separated list of GITCommits
        /// </summary>
        public string GITCommits
        {
            get { return this.Retrieve(x => x.GITCommits); }
            set { this.Store(x => x.GITCommits, value); }
        }
    }
}