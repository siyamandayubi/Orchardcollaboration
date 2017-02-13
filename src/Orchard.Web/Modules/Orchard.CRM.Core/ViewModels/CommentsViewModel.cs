namespace Orchard.CRM.Core.ViewModels
{
    using Orchard.CRM.Core.Models;
    using Orchard.Security;
    using System;
    using System.Collections.ObjectModel;

    public class CommentsViewModel
    {
        private Collection<CRMCommentViewModel> comments = new Collection<CRMCommentViewModel>();
        
        public int Id { get; set; }
        public int ContentItemId { get; set; }
        public int CommentsCount { get; set; }
        public IUser CurrentUser { get; set; }

        public Collection<CRMCommentViewModel> Comments
        {
            get
            {
                return this.comments;
            }
        }

        public class CRMCommentViewModel
        {
            public IUser User { get; set; }
            public virtual DateTime? CommentDateUtc { get; set; }
            public virtual string CommentText { get; set; }
            public virtual bool IsEmail { get; set; }
            public virtual bool IsHtml { get; set; }
            public virtual string CC { get; set; }
            public virtual string BCC { get; set; }
            public virtual string Subject { get; set; }

        }
    }
}