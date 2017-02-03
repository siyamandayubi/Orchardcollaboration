using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class DashboardViewModel
    {
        public int AllItemsWithoutOwnerCount { get; set; }
        public int CurrentUserOverrudeItemsCount { get; set; }
        public int CurrentUserOverrudeRequestingTicketCount { get; set; }
        public int AllOverrudeItemsCount { get; set; }
        public Collection<dynamic> CurrentUserTickets { get; set; }
        public Collection<dynamic> CurrentUserRequestingTickets { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsOperator { get; set; }
        public Collection<dynamic> AllTickets { get; set; }
        public int CurrentUserId { get; set; }
    }
}