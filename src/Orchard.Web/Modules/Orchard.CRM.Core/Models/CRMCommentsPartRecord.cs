using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class CRMCommentsPartRecord : ContentPartRecord
    {
        public virtual bool ThreadedComments { get; set; }
        public virtual int CommentsCount { get; set; }
    }
}