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