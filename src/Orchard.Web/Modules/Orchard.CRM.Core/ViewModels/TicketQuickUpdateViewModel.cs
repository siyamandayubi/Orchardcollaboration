using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class TicketQuickUpdateViewModel
    {
        public List<int> Ids { get; set; }

        public int? PriorityId { get; set; }
        public int? ServiceId { get; set; }
        public int? StatusId { get; set; }
        public DateTime? DueDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? UserId { get; set; }
        public int? BusinessUnitId { get; set; }
        public int? TeamId { get; set; }
        public int? TypeId { get; set; }

        public bool UpdateDescription { get; set; }
        public bool UpdatePriority { get; set; }
        public bool UpdateTypeId { get; set; }
        public bool UpdateServiceId { get; set; }
        public bool UpdateDueDate { get; set; }
        public bool UpdateStatusId { get; set; }
        public bool UpdateUserId { get; set; }
        public bool UpdateTeamId { get; set; }
        public bool UpdateBusinessUnitId { get; set; }
        public string returnUrl { get; set; }
    }
}