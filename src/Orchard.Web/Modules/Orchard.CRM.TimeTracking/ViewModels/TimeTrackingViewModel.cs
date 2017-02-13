using System;
using System.ComponentModel.DataAnnotations;

namespace Orchard.CRM.TimeTracking.ViewModels
{
    public class TimeTrackingViewModel
    {
        [Required]
        public int ContentItemId { get; set; }

        public int? TrackingItemId { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public int UserId { get; set; }

        [Required]
        public string TrackedTimeInString { get; set; }

        public int TimeInMinutes { get; set; }

        [Required]
        public DateTime TimeTrackDay { get; set; }
    }
}