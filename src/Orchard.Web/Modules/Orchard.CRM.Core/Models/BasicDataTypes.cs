
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