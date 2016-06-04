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
                model.Comments.Add(this.Convert(record, users));
            }

            return ContentShape("Parts_CRMComments",
                () => shapeHelper.Parts_CRMComments(
                    Model: model
                    ));
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
                model.Comments.Add(this.Convert(record, users));
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

        private CommentsViewModel.CRMCommentViewModel Convert(CRMCommentPartRecord record, IEnumerable<IUser> users)
        {
            return new CommentsViewModel.CRMCommentViewModel
            {
                IsEmail = record.IsEmail,
                Subject = record.Subject,
                BCC = record.BCC,
                CC = record.CC,
                CommentDateUtc = record.CommentDateUtc,
                CommentText = record.CommentText,
                IsHtml = record.IsHtml,
                User = users.FirstOrDefault(c => c.Id == record.User.Id)
            };
        }
    }
}