using System.Collections.Generic;
using Orchard.CRM.TimeTracking.Models;
using System;
using Orchard.CRM.TimeTracking.ViewModels;

namespace Orchard.CRM.TimeTracking.Services
{
    public interface ITimeTrackingService : IDependency
    {
        IEnumerable<TimeTrackingItemRecord> GetTimeTrackingItems(int contnentItemId);
        TimeTrackingItemRecord GetTimeTrackingItem(int id);
        TimeTrackingItemRecord Add(TimeTrackingViewModel model);
        void Delete(int id);
        TimeTrackingItemRecord Edit(TimeTrackingViewModel model);
    }
}
