using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class StatusRecordViewModel : BasicDataRecordViewModel
    {
        public int? StatusTypeId { get; set; }

        public int OrderId { get; set; }
    }
}