using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.CRM.Notification.Services
{
    public interface IActivityNotificationService: IDependency {
        ContentItem GetLatestCRMNotificationListItem();
        int NewItemsCount();
        void UpdateLastVisitActivity(int activityStreamId);
        IEnumerable<ActivityStreamRecord> Notifications(int pageId, int pageSize);
    }
}
