/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

namespace Orchard.CRM.Core.Services
{
    using Orchard.CRM.Core.Models;
    using Orchard.Data;
    using System;
    using System.Linq;
    using System.Web.Mvc;

    public class ValidationService : IValidationService
    {
        private readonly IRepository<StatusRecord> statusRepository;
        public ValidationService(IRepository<StatusRecord> statusRepository)
        {
            this.statusRepository = statusRepository;
        }

        public bool IsStatusTypeChangeValid(StatusRecord changedRecord, ModelStateDictionary modelState)
        {
            if (changedRecord == null) { throw new ArgumentNullException("changedRecord"); }
            if (modelState == null) { throw new ArgumentNullException("modelState"); }
            var statusRecords = this.statusRepository.Table.Where(c => c.Deleted == false).ToList();

            var statusNames = new Tuple<int, string>[] {
                new Tuple<int, string>(StatusRecord.NewStatus, "New"), 
                new Tuple<int, string>(StatusRecord.OpenStatus, "In Progress"), 
                new Tuple<int, string>(StatusRecord.DeferredStatus, "Deferred"), 
                new Tuple<int, string>(StatusRecord.PendingInputStatus, "Pending input"), 
                new Tuple<int, string>(StatusRecord.ClosedStatus, "Completed")};

            var oldRecord = statusRecords.FirstOrDefault(c => c.Id == changedRecord.Id);

            if (oldRecord != null)
            {
                statusRecords.Remove(oldRecord);
            }

            statusRecords.Add(changedRecord);

            var statusTypes = new[] { StatusRecord.NewStatus, StatusRecord.OpenStatus, StatusRecord.DeferredStatus, StatusRecord.PendingInputStatus, StatusRecord.ClosedStatus };

            foreach (var statusType in statusTypes)
            {
                var recordsWithThisStatusType = statusRecords.Where(c => c.StatusTypeId == statusType).ToList();

                if (recordsWithThisStatusType.Count > 1)
                {
                    modelState.AddModelError("StatusTypeId", "It is not allowed to have two status with the same StatusType");
                    return false;
                }

                if (recordsWithThisStatusType.Count == 0)
                {
                    var statusName = statusNames.First(c => c.Item1 == statusType).Item2;
                    modelState.AddModelError("StatusTypeId", "It is not allowed to remove a basic StatusType:" + statusName);
                    return false;
                }
            }

            return true;
        }
    }
}