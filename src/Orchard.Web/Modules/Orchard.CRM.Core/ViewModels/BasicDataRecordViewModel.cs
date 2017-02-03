using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class BasicDataRecordViewModel : IBasicDataRecord
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}