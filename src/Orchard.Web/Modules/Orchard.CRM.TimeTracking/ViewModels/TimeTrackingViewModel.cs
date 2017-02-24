using System;
using System.ComponentModel.DataAnnotations;

namespace Orchard.CRM.TimeTracking.ViewModels
{
    public class TimeTrackingViewModel
    {
        public const string TimeTrackingRegularExpressionPattern = "^(\\d+[d])?(\\s+\\d+[h])?(\\s+\\d+[m])?\\s*$";

        [Required]
        public int ContentItemId { get; set; }

        public int? TrackingItemId { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public int UserId { get; set; }

        public string FullUsername { get; set; }

        [Required]
        [RegularExpression(pattern: TimeTrackingRegularExpressionPattern)]
        public string TrackedTimeInString { get; set; }

        public int TimeInMinutes { get; set; }

        [Required]
        public DateTime TrackingDate { get; set; }

        public bool UserCanEdit { get; set; }
    }
}