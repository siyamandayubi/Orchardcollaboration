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


namespace Orchard.CRM.Core.Models
{
    public class ContentItemPermissionAccessTypes
    {
        public const byte Assignee = 1;
        public const byte SharedForView = 2;
        public const byte SharedForEdit = 3;
    }

    public class TicketSourceTypes
    {
        public const int CMS = 1;
        public const int Email = 2;
    }

    public class StatusRecord : IBasicDataRecord
    {
        public const int OpenStatus = 20;
        public const int ClosedStatus = 40;
        public const int DeferredStatus = 30;
        public const int PendingInputStatus = 35;
        public const int NewStatus = 10;

        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual bool IsHardCode { get; set; }
        public virtual int OrderId { get; set; }
        public virtual int? StatusTypeId { get; set; }
        public virtual bool Deleted { get; set; }
    }

    public class PriorityRecord : BasicDataRecord
    {
        public virtual int OrderId { get; set; }

        public const string LowPriority = "Low";
        public const string NormalPriority = "Medium";
        public const string HighPriority = "High";
        public const string CriticalPriority = "Critical";
    }

    public class TicketTypeRecord : BasicDataRecord
    {
        public virtual int OrderId { get; set; }
        public virtual bool Deleted { get; set; }
    }
}