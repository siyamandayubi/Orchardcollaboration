using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class TicketMenuItemPostViewModel
    {
        public int? StatusId { get; set; }
        public string GroupId { get; set; }
        public int? UserId { get; set; }
        public int? DueDateDays { get; set; }
    }
}