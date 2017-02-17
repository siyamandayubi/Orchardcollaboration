using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.CRM.TimeTracking.Models;
using Orchard.CRM.TimeTracking.ViewModels;
using Orchard.Data;
using Orchard.Users.Models;
using Orchard.ContentManagement;
using Orchard.Security;

namespace Orchard.CRM.TimeTracking.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly IRepository<TimeTrackingItemRecord> timeTrackingItemRepository;
        private readonly IOrchardServices services;

        public TimeTrackingService(IRepository<TimeTrackingItemRecord> timeTrackingItemRepository, IOrchardServices services)
        {
            this.services = services;
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
                TrackingDate = model.TrackingDate,
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
                record.TrackingDate = model.TrackingDate;

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

            var entities = records.ConvertAll(Convert).ToList();

            var userIds = records.Where(c => c.User != null).Select(c => c.User.Id).Distinct().ToArray();
            var users = this.services.ContentManager.GetMany<IUser>(userIds, VersionOptions.Published, new QueryHints());
            entities.ForEach(c =>
            {
                var user = users.FirstOrDefault(d => d.Id == c.UserId);
                if (user != null)
                {
                    c.FullUsername = Orchard.CRM.Core.Services.CRMHelper.GetFullNameOfUser(user);
                }
            });

            return entities;
        }

        private TimeTrackingViewModel Convert(TimeTrackingItemRecord c)
        {
            return new TimeTrackingViewModel
            {
                ContentItemId = c.TimeTrackingPartRecord.ContentItemRecord.Id,
                Comment = c.Comment,
                TrackingDate = c.TrackingDate,
                UserId = c.User.Id,
                TrackingItemId = c.Id,
                TrackedTimeInString = c.OriginalTimeTrackingString,
                TimeInMinutes = c.TimeInMinute
            };
        }
    }
}
