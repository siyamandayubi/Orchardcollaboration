using System.Collections.Generic;
using Orchard.CRM.TimeTracking.Models;
using System;
using Orchard.CRM.TimeTracking.ViewModels;

namespace Orchard.CRM.TimeTracking.Services
{
    public interface ITimeTrackingService : IDependency
    {
        IEnumerable<TimeTrackingViewModel> GetTimeTrackingItems(int contnentItemId);
        TimeTrackingViewModel GetTimeTrackingItem(int id);
        void Add(TimeTrackingViewModel model);
        void Delete(int id);
        void Edit(TimeTrackingViewModel model);
    }
}
