using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.ViewModels
{
    public class TicketViewModel : EditTicketViewModel
    {
        private Collection<SelectListItem> priorities = new Collection<SelectListItem>();
        private Collection<SelectListItem> statusItems = new Collection<SelectListItem>();
        private Collection<SelectListItem> services = new Collection<SelectListItem>();
        private Collection<SelectListItem> types = new Collection<SelectListItem>();
        private Collection<TicketViewModel> subTickets = new Collection<TicketViewModel>();
        private Collection<KeyValuePair<int, DateTime>> statusTimes = new Collection<KeyValuePair<int, DateTime>>();

        public bool CurrentUserCanEditItem { get; set; }
        public bool CurrentUserIsCustomer { get; set; }
        public bool CurrentUserCanChangePermission { get; set; }

        public string StatusName { get; set; }
        public string TypeName { get; set; }
        public string PriorityName { get; set; }
        public string ServiceName { get; set; }
        public int? TicketNumber { get; set; }
        public int? RelatedContentItemId { get; set; }
        public int? SourceId { get; set; }
        public string SourceData { get; set; }

        public string RequestingUserEmail { get; set; }
        public string RequestingUserFullName { get; set; }
        public string RequestingUsername { get; set; }

        public Collection<KeyValuePair<int, DateTime>> StatusTimes
        {
            get
            {
                return this.statusTimes;
            }
        }

        public Collection<TicketViewModel> SubTickets
        {
            get
            {
                return this.subTickets;
            }
        }

        public Collection<SelectListItem> Priorities
        {
            get
            {
                return this.priorities;
            }
        }

        public Collection<SelectListItem> Services
        {
            get
            {
                return this.services;
            }
        }

        public Collection<SelectListItem> StatusItems
        {
            get
            {
                return this.statusItems;
            }
        }

        public Collection<SelectListItem> Types
        {
            get
            {
                return this.types;
            }
        }
    }
}