using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class TicketMenuItemPartRecord : ContentPartRecord
    {
        public StatusRecord Status { get; set; }
        public BusinessUnitPartRecord BusinessUnit { get; set; }
        public TeamPartRecord Team { get; set; }
        public UserPartRecord User { get; set; }

        /// <summary>
        /// Number of days to be added to current date to get the due date
        /// </summary>
        public virtual int? DueDateDays { get; set; }
    }
}