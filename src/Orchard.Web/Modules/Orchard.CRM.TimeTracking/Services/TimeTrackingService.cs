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

        public void Add(TimeTrackingViewModel model)
        {
            TimeTrackingItemRecord record = new TimeTrackingItemRecord
            {
                Comment = model.Comment,
                OriginalTimeTrackingString = model.TrackedTimeInString,
                TimeInMinute = model.TimeInMinutes,
                TimeTrackingPartRecord = new TimeTrackingPartRecord { Id = model.ContentItemId },
                TrackingDate = model.TimeTrackDay,
                User = new Users.Models.UserPartRecord { Id = model.UserId }
            };

            this.timeTrackingItemRepository.Create(record);
            this.timeTrackingItemRepository.Flush();
        }

        public void Delete(int id)
        {
            var record = this.timeTrackingItemRepository.Table.FirstOrDefault(c => c.Id == id);
            if (record != null)
            {
                this.timeTrackingItemRepository.Delete(record);
            }
            this.timeTrackingItemRepository.Flush();
        }

        public void Edit(TimeTrackingViewModel model)
        {
            var record = this.timeTrackingItemRepository.Table.FirstOrDefault(c => c.Id == model.TrackingItemId);
            if (record != null)
            {
                record.Comment = model.Comment;
                record.TimeInMinute = model.TimeInMinutes;
                record.OriginalTimeTrackingString = model.TrackedTimeInString;
                record.TrackingDate = model.TimeTrackDay;

                if (record.User != null)
                {
                    record.User.Id = model.UserId;
                }
                else
                {
                    record.User = new Users.Models.UserPartRecord { Id = model.UserId };
                }
            }
            this.timeTrackingItemRepository.Flush();
        }

        public TimeTrackingViewModel GetTimeTrackingItem(int id)
        {
            var record = this.timeTrackingItemRepository.Table.FirstOrDefault(c => c.Id == id);
            return record != null ? Convert(record) : null;
        }

        public IEnumerable<TimeTrackingViewModel> GetTimeTrackingItems(int contnentItemId)
        {
            var records = this.timeTrackingItemRepository.Table.Where(c => c.TimeTrackingPartRecord.ContentItemRecord.Id == contnentItemId).ToList();

            return records.ConvertAll(Convert);
        }

        private TimeTrackingViewModel Convert(TimeTrackingItemRecord c)
        {
            return new TimeTrackingViewModel
            {
                ContentItemId = c.TimeTrackingPartRecord.ContentItemRecord.Id,
                Comment = c.Comment,
                TimeTrackDay = c.TrackingDate,
                UserId = c.User.Id,
                TrackingItemId = c.Id,
                TrackedTimeInString = c.OriginalTimeTrackingString,
                TimeInMinutes = c.TimeInMinute
            };
        }
    }
}
