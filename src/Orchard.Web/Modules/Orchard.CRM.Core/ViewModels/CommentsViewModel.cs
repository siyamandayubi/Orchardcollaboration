/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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