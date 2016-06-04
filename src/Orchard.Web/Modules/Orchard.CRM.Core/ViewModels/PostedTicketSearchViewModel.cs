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

namespace Orchard.CRM.Core.ViewModels
{
    using System.Collections.ObjectModel;

    public class PostedTicketSearchViewModel
    {
        public const string OverDueDate = "Overdue";

        public string Status { get; set; }
        public string Term { get; set; }
        public string DueDate { get; set; }

        public bool UnStatus { get; set; }
        public bool Unassigned { get; set; }

        public int? ProjectId { get; set; }

        /// <summary>
        /// only admin users can set this parameter to true
        /// </summary>
        public bool IncludeAllVisibleItemsBySelectedGroupsAndUsers { get; set; }

        public int[] Users { get; set; }
        public int[] BusinessUnits { get; set; }

        public int? RelatedContentItemId { get; set; }
    }
}