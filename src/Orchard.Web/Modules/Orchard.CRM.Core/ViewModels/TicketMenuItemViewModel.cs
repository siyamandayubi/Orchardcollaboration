using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.ViewModels
{
    public class TicketMenuItemViewModel
    {
        public TicketMenuItemViewModel(
            ICollection<SelectListItem> statusItems,
            ICollection<SelectListItem> dueDateDays,
            ICollection<SelectListItem> users,
            ICollection<BusinessUnitViewModel> businessUnits ,
            ICollection<TeamViewModel> teams
            )
        {
            this.StatusItems = statusItems;
            this.BusinessUnits = businessUnits;
            this.Teams = teams;
            this.Users = users;
            this.DueDateDays = dueDateDays;
        }

        public ICollection<SelectListItem> StatusItems { get; private set; }
        public ICollection<SelectListItem> DueDateDays { get; private set; }
        public ICollection<SelectListItem> Users { get; private set; }
        public ICollection<BusinessUnitViewModel> BusinessUnits { get; private set; }
        public ICollection<TeamViewModel> Teams { get; private set; }
    }
}