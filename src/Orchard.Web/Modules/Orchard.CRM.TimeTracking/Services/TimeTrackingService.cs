using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.CRM.TimeTracking.Models;
using Orchard.CRM.TimeTracking.ViewModels;
using Orchard.Data;
using Orchard.Users.Models;
using Orchard.ContentManagement;
using Orchard.Security;
using System.Text.RegularExpressions;

namespace Orchard.CRM.TimeTracking.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        public const string TimeFormat = "^(\\d[d])?(\\s*\\d[h])?(\\s*\\d[m])?\\s*$";

        private readonly IRepository<TimeTrackingItemRecord> timeTrackingItemRepository;
        private readonly IOrchardServices services;

        public TimeTrackingService(IRepository<TimeTrackingItemRecord> timeTrackingItemRepository, IOrchardServices services)
        {
            this.services = services;
            this.timeTrackingItemRepository = timeTrackingItemRepository;
        }

        public void Add(TimeTrackingViewModel model)
        {
            var matches = Regex.Matches(model.TrackedTimeInString, TimeFormat);

            model.TimeInMinutes = 0;
            if (matches.Count > 0)
            {
                model.TimeInMinutes = ConvertTimeSpanStringToMinutes(model.TrackedTimeInString);
            }

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
            model.TrackingItemId = record.Id;
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
                var matches = Regex.Matches(model.TrackedTimeInString, TimeFormat);

                model.TimeInMinutes = 0;
                if (matches.Count > 0)
                {
                    model.TimeInMinutes = ConvertTimeSpanStringToMinutes(model.TrackedTimeInString);
                }

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

        private int ConvertTimeSpanStringToMinutes(string timeSpan)
        {
            var days = Regex.Match(timeSpan, "(\\d[d])");
            var hours = Regex.Match(timeSpan, "(\\d[h])");
            var minutes = Regex.Match(timeSpan, "(\\d[m])");

            int total = 0;
            if (days.Success)
            {
                string daysInStr = timeSpan.Substring(days.Index, days.Length);
                daysInStr = daysInStr.Substring(0, daysInStr.Length - 1);
                total += int.Parse(daysInStr) * 24 * 60;
            }

            if (hours.Success)
            {
                string hoursInStr = timeSpan.Substring(hours.Index, hours.Length);
                hoursInStr = hoursInStr.Substring(0, hoursInStr.Length - 1);
                total += int.Parse(hoursInStr) * 60;
            }

            if (minutes.Success)
            {
                string minutesInStr = timeSpan.Substring(minutes.Index, minutes.Length);
                minutesInStr = minutesInStr.Substring(0, minutesInStr.Length - 1);
                total += int.Parse(minutesInStr);
            }

            return total;
        }
    }
}
