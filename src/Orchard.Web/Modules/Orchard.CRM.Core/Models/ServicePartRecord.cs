using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;

namespace Orchard.CRM.Core.Models
{
    public class ServicePart : ContentPart<ServicePartRecord>
    {
        public string Name
        {
            get { return this.Record.Name; }
            set { this.Record.Name = value; }
        }

        public string Description
        {
            get { return this.Record.Description; }
            set { this.Record.Description = value; }
        }
    }

    public class ServicePartRecord : ContentPartRecord, IBasicDataRecord
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
    }
}