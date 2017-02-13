using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class AttachToProjectPartRecord : ContentPartRecord
    {
        [Aggregate]
        public virtual ProjectPartRecord Project { get; set; }
    }
}