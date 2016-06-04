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

namespace Orchard.CRM.Core.Handlers
{
    using Orchard.ContentManagement.Handlers;
    using Orchard.CRM.Core.Models;
    using Orchard.Data;
    using Orchard.ContentManagement;
    using System.Linq;

    public class CRMCommentsHandler : ContentHandler
    {
        public CRMCommentsHandler(IRepository<CRMCommentsPartRecord> commentsRepository, IRepository<CRMCommentPartRecord> commentRepository)
        {
            Filters.Add(StorageFilter.For(commentsRepository));

            OnIndexing<CRMCommentsPart>((context, crmCommentsPart) =>
            {
                var comments = commentRepository.Table.Where(c => c.CRMCommentsPartRecord.Id == crmCommentsPart.Record.Id).ToList();
                string commentsString = string.Join(" ", comments.Select(c => c.CommentText));

                context.DocumentIndex.Add(CRMCommentsPart.CommentsFieldName, commentsString).Analyze().Store();
            });
        }
    }
}