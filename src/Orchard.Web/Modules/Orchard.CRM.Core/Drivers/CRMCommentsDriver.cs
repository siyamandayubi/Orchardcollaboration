using Orchard.CRM.Core.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.CRM.Core.ViewModels;
using Orchard.Security;
using Orchard.CRM.Core.Services;

namespace Orchard.CRM.Core.Drivers
{
    public class CRMCommentsDriver : ContentPartDriver<CRMCommentsPart>
    {
        private readonly IRepository<CRMCommentPartRecord> commentPartRecordRepository;
        private readonly IOrchardServices services;

        public CRMCommentsDriver(
            IRepository<CRMCommentPartRecord> commentPartRecordRepository,
            IOrchardServices services)
        {
            this.services = services;
            this.commentPartRecordRepository = commentPartRecordRepository;
        }

        protected override DriverResult Display(CRMCommentsPart part, string displayType, dynamic shapeHelper)
        {
            CommentsViewModel model = new CommentsViewModel();

            model.Id = part.Record.Id;
            model.CurrentUser = this.services.WorkContext.CurrentUser;
            model.ContentItemId = part.ContentItem.Id;
            model.CommentsCount = part.Record.CommentsCount;

            if (displayType == "TableRow" || displayType == "Summary" || displayType == "SummaryAdmin" || part.Record.Id == default(int))
            {
                return ContentShape("Parts_CRMComments_Summary",
                       () => shapeHelper.Parts_CRMComments_Summary(
                           Model: model
                           ));
            }

            var comments = this.commentPartRecordRepository.Table.Where(c => c.CRMCommentsPartRecord.Id == part.Record.Id).ToList();
            var users = this.services.ContentManager.GetMany<IUser>(comments.Where(c => c.User != null).Select(c => c.User.Id), VersionOptions.Published, QueryHints.Empty);
            foreach (var record in comments)
            {
                model.Comments.Add(Converter.Convert(services, record, users));
            }

            var result = new List<DriverResult>();

            result.Add(ContentShape("Parts_CRMComments", () => shapeHelper.Parts_CRMComments(Model: model)));
            result.Add(ContentShape("Parts_CRMComments_Header", () => shapeHelper.Parts_CRMComments_Header(Model: model)));

            return Combined(result.ToArray());
        }

        protected override DriverResult Editor(CRMCommentsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            // refresh commits count
            part.Record.CommentsCount = this.commentPartRecordRepository.Table.Count(c => c.CRMCommentsPartRecord.Id == part.Record.Id);

            return this.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(CRMCommentsPart part, dynamic shapeHelper)
        {
            CommentsViewModel model = new CommentsViewModel();

            var comments = this.commentPartRecordRepository.Table.Where(c => c.CRMCommentsPartRecord.Id == part.Record.Id).ToList();
            var users = this.services.ContentManager.GetMany<IUser>(comments.Where(c => c.User != null).Select(c => c.User.Id), VersionOptions.Published, QueryHints.Empty);
            model.Id = part.Record.Id;
            model.CommentsCount = part.Record.CommentsCount;

            foreach (var record in comments)
            {
                model.Comments.Add(Converter.Convert(services, record, users));
            }

            return this.Combined(
                    ContentShape("Parts_CRMComments_Edit",
                        () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/CRMComments",
                        Model: model,
                        Prefix: Prefix)),
                    ContentShape("Parts_CRMComments",
                        () => shapeHelper.Parts_CRMComments(
                            Model: model,
                            Prefix: Prefix))
                        );
        }
    }
}