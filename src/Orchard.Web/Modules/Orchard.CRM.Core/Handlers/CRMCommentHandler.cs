using Orchard.CRM.Core.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Indexing;
using Orchard.CRM.Core.Controllers;

namespace Orchard.CRM.Core.Handlers
{
    public class CRMCommentHandler : ContentHandler
    {
        public CRMCommentHandler(IRepository<CRMCommentPartRecord> commentRepository, IContentManager contentManager, IIndexProvider indexProvider)
        {
            Filters.Add(StorageFilter.For(commentRepository));

            this.OnPublishing<CRMCommentPart>((context, part) =>
            {
                part.Record.CommentDateUtc = DateTime.UtcNow;

                part.Record.CRMCommentsPartRecord.CommentsCount++;
            });

            this.OnRemoved<CRMCommentPart>((context, part) =>
            {
                part.Record.CRMCommentsPartRecord.CommentsCount--;
            });

            this.OnIndexing<CRMCommentPart>((context, part) =>
            {
            });
        }
    }
}