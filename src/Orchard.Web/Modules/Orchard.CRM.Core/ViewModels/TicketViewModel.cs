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