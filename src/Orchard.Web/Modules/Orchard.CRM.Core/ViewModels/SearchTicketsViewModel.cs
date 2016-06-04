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
using Orchard.Localization;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.ViewModels
{
    public class SearchTicketsViewModel
    {
        private Collection<BusinessUnitViewModel> businessUnits = new Collection<BusinessUnitViewModel>();
        private Collection<UserViewModel> users = new Collection<UserViewModel>();
        private Collection<SelectListItem> projects = new Collection<SelectListItem>();
        private Collection<KeyValuePair<DateTime, string>> dueDates = new Collection<KeyValuePair<DateTime, string>>();

        public bool IsThereAnySelectedUserOrGroup
        {
            get
            {
                return this.users.Any(c => c.Checked) || this.businessUnits.Any(c => c.Checked || c.Teams.Any(d => d.Checked));
            }
        }

        public Collection<SelectListItem> Projects
        {
            get
            {
                return this.projects;
            }
        }

        public bool IsProjectForTicketsSupported { get; set; }
        public int? ProjectId { get; set; }

        public int ClosedStatusId { get; set; }
        public int OpenStatusId { get; set; }
        public bool IsAdminUser { get; set; }
        public bool IsCustomerUser { get; set; }
        public string StatusId { get; set; }
        public dynamic Pager { get; set; }
        public List<dynamic> Items { get; set; }
        public bool IncludeAllVisibleItemsBySelectedGroupsAndUsers { get; set; }
        public bool Unassigned { get; set; }
        public string Term { get; set; }
        public DateTime? DueDate { get; set; }
        public bool UnStatus { get; set; }
        public bool Overdue { get; set; }
        public int? RelatedContentItemId { get; set; }
        public string RelatedContentItemTitle { get; set; }
        public PagerParametersWithSortFields PagerParameters { get; set; }

        public Collection<BusinessUnitViewModel> BusinessUnits
        {
            get
            {
                return this.businessUnits;
            }
        }

        public Collection<UserViewModel> Users
        {
            get
            {
                return this.users;
            }
        }

        public string SearchDescription { get; set; }

        public class UserViewModel
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public bool Checked { get; set; }
            public bool IsAdminOrOperator { get; set; }
        }
    }
}