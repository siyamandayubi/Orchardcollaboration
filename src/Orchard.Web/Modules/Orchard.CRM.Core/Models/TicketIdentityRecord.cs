using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class TicketIdentityRecord
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// At the moment, we don't use this field, we have to have it, because  NHIbernate can not insert
        /// a record with only one identity field into SQL Compact 
        /// </summary>
        public virtual string TempData { get; set; }
    }
}