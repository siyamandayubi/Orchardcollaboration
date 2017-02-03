using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    [Obsolete]
    public class ServiceRecord : IBasicDataRecord
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual bool Deleted { get; set; }
    }
}