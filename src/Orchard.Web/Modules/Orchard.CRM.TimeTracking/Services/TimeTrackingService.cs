using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.CRM.TimeTracking.Models;
using Orchard.CRM.TimeTracking.ViewModels;
using Orchard.Data;

namespace Orchard.CRM.TimeTracking.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly IRepository<TimeTrackingItemRecord> timeTrackingItemRepository;

        public TimeTrackingService(IRepository<TimeTrackingItemRecord> timeTrackingItemRepository)
        {
            this.timeTrackingItemRepository = timeTrackingItemRepository;
        }

        public TimeTrackingItemRecord Add(TimeTrackingViewModel model)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public TimeTrackingItemRecord Edit(TimeTrackingViewModel model)
        {
            throw new NotImplementedException();
        }

        public TimeTrackingItemRecord GetTimeTrackingItem(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TimeTrackingItemRecord> GetTimeTrackingItems(int contnentItemId)
        {
            return this.timeTrackingItemRepository.Table.Where(c => c.TimeTrackingPartRecord.ContentItemRecord.Id == contnentItemId).ToList();
        }

        public TimeTrackingItemRecord LogTime(int userId, string time, DateTime dateTime, string comment)
        {
            return null;
        }
    }
}
