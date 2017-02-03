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