using Orchard.Users.Models;
using System;

namespace Orchard.CRM.TimeTracking.Models
{
    public class TimeTrackingItemRecord
    {
        public virtual int Id { get; set; }
        public virtual UserPartRecord User { get; set; }
        public virtual TimeTrackingPartRecord TimeTrackingPartRecord { get; set; }
        public virtual int TimeInMinute { get; set; }
        public virtual string Comment { get; set; }
        public virtual DateTime TrackingDate { get; set; }
    }
}