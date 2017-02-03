using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class CRMCommentPartRecord : ContentPartRecord
    {
        [Aggregate]
        public virtual UserPartRecord User { get; set; }
        public virtual CRMCommentsPartRecord CRMCommentsPartRecord { get; set; }

        public virtual DateTime? CommentDateUtc { get; set; }
        
        [StringLengthMax]
        public virtual string CommentText { get; set; }

        public virtual bool IsEmail { get; set; }
        public virtual bool IsHtml { get; set; }
        public virtual string CC { get; set; }
        public virtual string BCC { get; set; }
        public virtual string Subject { get; set; }

        /// <summary>
        /// NHibernate raises error for column "To", so we rename it to MTo
        /// </summary>
        public virtual string MTo { get; set; }

        /// <summary>
        /// NHibernate raises error for column "From", so we rename it to MFrom
        /// </summary>
        public virtual string MFrom { get; set; }
    }
}