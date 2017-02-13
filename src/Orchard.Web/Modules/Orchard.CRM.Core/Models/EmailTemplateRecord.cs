using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class EmailTemplateRecord
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Body { get; set; }
        public virtual string Subject { get; set; }
        public virtual int TypeId { get; set; }
    }
}