namespace Orchard.CRM.Core.Models
{
    using Orchard.ContentManagement;
    using System;
    using System.Collections.Generic;

    public class TicketPart : ContentPart<TicketPartRecord>
    {
        public const string NullValueForIntegers = "-1";
        public const string StatusFieldName = "status";
        public const string StatusOrderFieldName = "statusOrder";
        public const string TitleFieldName = "Title";
        public const string DescriptionFieldName = "Description";
        public const string PriorityFieldName = "Priority";
        public const string PriorityOrderFieldName = "PriorityOrder";
        public const string ServiceFieldName = "Service";
        public const string TypeFieldName = "TicketType";
        public const string IdentityFieldName = "Identity";
        public const string DueDateFieldName = "DueDate";
        public const string RequestingUserFieldName = "RequestingUser";
        public const string RelatedContentItemIdFieldName = "RelatedContentItemId";

        public string Description
        {
            get
            {
                return this.Record.Description;
            }
            set
            {
                this.Record.Description = value;
            }
        }

        public virtual PriorityRecord PriorityRecord
        {
            get
            {
                return this.Record.PriorityRecord;
            }
            set
            {
                this.Record.PriorityRecord = value;
            }
        }

        public virtual StatusRecord StatusRecord
        {
            get
            {
                return this.Record.StatusRecord;
            }
            set
            {
                this.Record.StatusRecord = value;
            }
        }

        public IList<KeyValuePair<int, DateTime>> StatusTimes
        {
            get
            {
                // The array has been stored as a JSON string, because Dictionary is not supported by retrieve method
                string serializedData = Retrieve<string>("StatusTimes");
                if (string.IsNullOrEmpty(serializedData))
                {
                    return new List<KeyValuePair<int, DateTime>>();
                }

                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<KeyValuePair<int, DateTime>>>(serializedData);
            }
            set
            {
                // The array has been stored as a comma sperated list, because List is not supported by Reterive method
                string serializedData = string.Empty;
                if (value != null)
                {
                    serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                }

                Store("StatusTimes", serializedData);
            }
        }
    }
}