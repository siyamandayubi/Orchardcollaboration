using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class EditTicketViewModel
    {
        public int? TicketId { get; set; }
        public int ContentItemId { get; set; }

        [MaxLength(5000)]
        public string Text { get; set; }
        public int? RelatedContentItemId { get; set; }
        public int? ParentTicketId { get; set; }
        public int? ParentTicketNumber { get; set; }
        public string ParentTicketTitle { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public int? StatusId { get; set; }

        public int? ServiceId { get; set; }

        public int? PriorityId { get; set; }

        public int? TypeId { get; set; }
    }
}