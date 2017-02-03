using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public interface IBasicDataRecord
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    public abstract class BasicDataRecord : IBasicDataRecord
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual bool IsHardCode { get; set; }
    }
}