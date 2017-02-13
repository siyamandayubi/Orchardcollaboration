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