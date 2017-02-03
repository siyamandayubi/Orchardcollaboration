using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Mvc;
using System.Linq;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Core.Common.Models;
using Orchard.Security;
using System;

namespace Orchard.CRM.Core.Services
{
    public static class Converter
    {
        public static CommentsViewModel.CRMCommentViewModel Convert(IOrchardServices services, CRMCommentPartRecord record, IEnumerable<IUser> users)
        {
            var output = new CommentsViewModel.CRMCommentViewModel
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

            if (output.CommentDateUtc.HasValue && output.CommentDateUtc.Value.Kind == DateTimeKind.Utc)
            {
                output.CommentDateUtc = CRMHelper.SetSiteTimeZone(services, output.CommentDateUtc.Value);
            }

            return output;
        }

        public static void Fill(Collection<SelectListItem> collection, int? selectedValue, IEnumerable<IBasicDataRecord> items)
        {
            foreach (var item in items)
            {
                bool isSelected = selectedValue.HasValue && selectedValue.Value == item.Id;
                collection.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(),
                    Selected = isSelected
                });
            }
        }

        public static void Fill(Collection<SelectListItem> target, IEnumerable<ProjectPart> projects)
        {
            target.AddRange(projects.Select(c =>
                new SelectListItem
                {
                    Text = c.Record.Title,
                    Value = c.Record.Id.ToString(CultureInfo.InvariantCulture)
                }));

            target.Insert(0, new SelectListItem());
        }

        public static void FillByIdentity(Collection<SelectListItem> target, IEnumerable<ProjectPart> projects)
        {
            target.AddRange(projects.Select(c =>
            {
                var item = new SelectListItem
                {
                    Text = c.Record.Title,
                    Value = c.Record.Id.ToString(CultureInfo.InvariantCulture)
                };

                var identity = c.As<IdentityPart>();
                if (identity != null)
                {
                    item.Value = identity.Identifier;
                }

                return item;
            }));

            target.Insert(0, new SelectListItem());
        }

        public static void Fill(Collection<SelectListItem> target, IEnumerable<ContentItem> contentItems)
        {
            Fill(target, contentItems, true);
        }

        public static void Fill(Collection<SelectListItem> target, IEnumerable<ContentItem> contentItems, bool addEmptyItem)
        {
            target.AddRange(contentItems.Select(c =>
                new SelectListItem
                {
                    Text = c.Is<TitlePart>() ? c.As<TitlePart>().Title : c.Id.ToString(),
                    Value = c.Record.Id.ToString(CultureInfo.InvariantCulture)
                }));

            if (addEmptyItem)
            {
                target.Insert(0, new SelectListItem());
            }
        }

        public static TargetContentItemPermissionViewModel DecodeGroupId(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return null;
            }

            string[] parts = groupId.Split(new[] { ':' });
            if (parts.Length > 0)
            {
                int id = int.Parse(parts[1]);
                TargetContentItemPermissionViewModel targetContentItemPermissionViewModel = new TargetContentItemPermissionViewModel { Checked = true };
                if (parts[0] == "Team")
                {
                    targetContentItemPermissionViewModel.TeamId = id;
                }
                else
                {
                    targetContentItemPermissionViewModel.BusinessUnitId = id;
                }

                return targetContentItemPermissionViewModel;
            }
            else
            {
                return null;
            }
        }
    }
}