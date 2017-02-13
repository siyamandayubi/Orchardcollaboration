using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Users.Models;
using Orchard.CRM.Core.Models;

namespace Orchard.CRM.Notification.Models
{
    public class UserContentVisitRecord
    {
        public virtual int Id { get; set; }
        public virtual UserPartRecord User { get; set; }
        public virtual int LastVisitedActivityStreamId { get; set; }
        public virtual DateTime? LastVisitTime { get; set; }
    }
}