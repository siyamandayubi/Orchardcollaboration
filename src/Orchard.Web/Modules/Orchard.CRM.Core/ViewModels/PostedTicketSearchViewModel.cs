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