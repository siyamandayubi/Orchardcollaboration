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

namespace Orchard.CRM.Core.Providers
{
    using Newtonsoft.Json;
    using Orchard.ContentManagement;
    using Orchard.Core.Title.Models;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.Localization;
    using Orchard.Tokens;
    using Orchard.Users.Models;
    using S22.IMAP.Models;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class ActivityTokenProvider : ITokenProvider
    {
        private Localizer T { get; set; }
        private readonly IContentManager contentManager;
        private readonly IBasicDataService basicDataService;

        /// <summary>
        /// The key of TitlePart in the EvaluateContext.Data dictionary
        /// </summary>
        public const string TitkeKey = "ETitlePart";

        /// <summary>
        /// The key of CRMCommentPart in the EvaluateContext.Data dictionary
        /// </summary>
        public const string CRMCommentKey = "EComment";

        /// <summary>
        /// The key of TicketPart in the EvaluateContext.Data dictionary
        /// </summary>
        public const string TicketKey = "ETicket";

        /// <summary>
        /// The key of UserPart in the EvaluateContext.Data dictionary
        /// </summary>
        public const string UserKey = "EUser";

        /// <summary>
        /// The key of BusinessUnitPart in the EvaluateContext.Data dictionary
        /// </summary>
        public const string BusinessUnitKey = "EBusinessUnit";

        /// <summary>
        /// The key of TeamPart in the EvaluateContext.Data dictionary
        /// </summary>
        public const string TeamKey = "ETeam";

        /// <summary>
        /// The key of EmailPart
        /// </summary>
        public const string EEmailKey = "EEmail";

        /// <summary>
        /// The key of ContentItemPermissionDetailRecord in the EvaluateContext.Data dictionary
        /// </summary>
        public const string PermissionDetailKey = "EPermissionDetail";

        /// <summary>
        /// The key of ContentItemPermissionRecord in the EvaluateContext.Data dictionary
        /// </summary>
        public const string PermissionPartKey = "EPermissionPart";

        public ActivityTokenProvider(
            IContentManager contentManager,
            IBasicDataService basicDataService
        )
        {
            this.contentManager = contentManager;
            this.T = NullLocalizer.Instance;
            this.basicDataService = basicDataService;
        }

        public void Describe(DescribeContext context)
        {
            context.For(ActivityTokenProvider.TitkeKey, T("ETitle"), T("Title"))
                 .Token("Title", T("Title"), T("Title"));

            context.For(ActivityTokenProvider.EEmailKey, T("EEmail"), T("EEmail"))
                 .Token("Subject", T("Subject"), T("Subject"))
                 .Token("Body", T("Body"), T("Body"))
                 .Token("UserId", T("UserId"), T("UserId"))
                 .Token("UserName", T("UserName"), T("UserName"))
                 .Token("From", T("From"), T("From"));

            context.For(ActivityTokenProvider.CRMCommentKey, T("EComment"), T("Comment"))
                .Token("CommentText", T("CommentText"), T("CommentText"));

            context.For(ActivityTokenProvider.UserKey, T("EUser"), T("User"))
                 .Token("Email", T("Email"), T("Email"))
                 .Token("Username", T("Username"), T("Username"));

            context.For(ActivityTokenProvider.TeamKey, T("ETeam"), T("ETeam"))
                .Token("Id", T("Id"), T("Id"));

            context.For(ActivityTokenProvider.BusinessUnitKey, T("EBusinessUnit"), T("EBusinessUnit"))
                .Token("Id", T("Id"), T("Id"));

            context.For(ActivityTokenProvider.TicketKey, T("ETicket"), T("ETicket"))
                .Token("Id", T("Id"), T("Id"))
                .Token("Number", T("Number"), T("Number"))
                .Token("RequestingUserEmail", T("RequestingUserEmail"), T("RequestingUserEmail"))
                .Token("RequestingUserName", T("RequestingUserName"), T("RequestingUserName"))
                .Token("RequestingUserFullName", T("RequestingUserFullName"), T("RequestingUserFullName"))
                .Token("Service", T("Service"), T("Service"))
                .Token("Priority", T("Priority"), T("Priority"))
                .Token("TicketType", T("TicketType"), T("TicketType"))
                .Token("Title", T("Title"), T("Title"))
                .Token("Status", T("Status"), T("Status"));
        }

        public void Evaluate(EvaluateContext context)
        {
            // Title
            context.For(ActivityTokenProvider.TitkeKey, () => this.GetPart<TitlePart>(context))
                .Token("Title", c =>
                {
                    TitlePart titlePart = this.GetPart<TitlePart>(context);

                    if (titlePart == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return titlePart.Title;
                    }
                });

            // User
            context.For(ActivityTokenProvider.UserKey, () => (UserPartRecord)context.Data[UserKey])
                .Token("Email", c =>
                {
                    var part = (UserPartRecord)context.Data[UserKey];
                    if (part == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return part.Email;
                    }
                })
                .Token("Username", c =>
                {
                    var part = (UserPartRecord)context.Data[UserKey];
                    if (part == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return part.Email;
                    }
                })
                .Token("FullName", contextParameter =>
                {
                    var part = (UserPartRecord)context.Data[UserKey];
                    if (part == null)
                    {
                        return string.Empty;
                    }

                    var userContentItem = contentManager.Get(part.Id);

                    // Sometimes the passed UserRecord is not persisted in the database or it is a faked object,
                    // so we have to check for nullability
                    if (userContentItem != null)
                    {
                        return CRMHelper.GetFullNameOfUser(userContentItem.As<UserPart>());
                    }
                    else
                    {
                        return part.UserName;
                    }
                });

            // Ticket
            this.SubstituteTicketProperties(context);

            // Email
            this.SubstituteEmailProperties(context);

            // Team
            this.CheckForContentPartId<TeamPart>(context, TeamKey);

            // BusinessUnit
            this.CheckForContentPartId<BusinessUnitPart>(context, BusinessUnitKey);

            // CRMComment
            context.For(ActivityTokenProvider.CRMCommentKey, () => this.GetPart<CRMCommentPart>(context))
                        .Token("CommentText", c =>
                        {
                            var part = this.GetPart<CRMCommentPart>(context);
                            if (part == null)
                            {
                                return string.Empty;
                            }
                            else
                            {
                                return part.Record.CommentText;
                            }
                        });
        }

        private TPart GetPart<TPart>(EvaluateContext context)
            where TPart : ContentPart
        {
            ContentItem contentItem = (ContentItem)context.Data["Content"];
            if (contentItem == null)
            {
                return null;
            }

            return contentItem.As<TPart>();
        }

        private TicketPart GetTicketPart(EvaluateContext context)
        {
            ContentItem contentItem = (ContentItem)context.Data["Content"];
            TicketPart ticketPart = contentItem.As<TicketPart>();
            if (ticketPart == null)
            {
                CRMCommentPart crmComment = contentItem.As<CRMCommentPart>();
                if (crmComment == null)
                {
                    return null;
                }
                else
                {
                    return this.contentManager.Get<TicketPart>(crmComment.Record.CRMCommentsPartRecord.ContentItemRecord.Id);
                }
            }

            return ticketPart;
        }

        private void SubstituteEmailProperties(EvaluateContext context)
        {
            var part = this.GetPart<IMAPEmailPart>(context);
            if (part == null)
            {
                return;
            }

            context.For(ActivityTokenProvider.EEmailKey, () => this.GetPart<IMAPEmailPart>(context))
                .Token("Subject", contextParameter =>
                {
                    return part.Subject;
                })
                .Token("Body", contextParameter =>
                {
                    string returnValue = part.MailMessage != null ? part.MailMessage.Body : string.Empty;
                    returnValue = part.MailMessage.IsBodyHtml ? returnValue : string.Format("<pre>{0}</pre>", returnValue);
                    return returnValue;
                })
               .Token("From", contextParameter =>
               {
                   return part.From;
               })
               .Token("UserId", contextParameter =>
               {
                   dynamic sender = JsonConvert.DeserializeObject(part.From);
                   return sender.UserId;
               })
               .Token("UserName", contextParameter =>
               {
                   dynamic sender = JsonConvert.DeserializeObject(part.From);
                   return sender.UserName;
               });
        }

        private void SubstituteTicketProperties(EvaluateContext context)
        {
            context.For(ActivityTokenProvider.TicketKey, () => this.GetTicketPart(context))
                .Token("Status", contextParameter =>
                {
                    var c = this.GetTicketPart(context);
                    if (c == null || c.StatusRecord == null)
                    {
                        return string.Empty;
                    }

                    var statusRecords = this.basicDataService.GetStatusRecords().ToList().Select(d => new BasicDataRecordViewModel { Id = d.Id, Name = d.Name }).ToList();
                    return this.GetBasicDataRecordName(c.StatusRecord.Id, statusRecords);

                })
                .Token("Service", contextParameter =>
                {
                    var c = this.GetTicketPart(context);
                    if (c == null || c.Record.Service == null)
                    {
                        return string.Empty;
                    }

                    var records = this.basicDataService.GetServices().ToList();
                    return this.GetBasicDataRecordName(c.Record.Service.Id, records);

                })
               .Token("RequestingUserEmail", contextParameter =>
               {
                   var c = this.GetTicketPart(context);
                   if (c == null || c.Record.RequestingUser == null)
                   {
                       return string.Empty;
                   }

                   return c.Record.RequestingUser.Email;

               })
               .Token("RequestingUserName", contextParameter =>
               {
                   var c = this.GetTicketPart(context);
                   if (c == null || c.Record.RequestingUser == null)
                   {
                       return string.Empty;
                   }

                   return c.Record.RequestingUser.UserName;

               })
               .Token("RequestingUserFullName", contextParameter =>
               {
                   var c = this.GetTicketPart(context);
                   if (c == null || c.Record.RequestingUser == null)
                   {
                       return string.Empty;
                   }

                   var userContentItem = contentManager.Get(c.Record.RequestingUser.Id);

                   string fieldValue = CRMHelper.GetFullNameOfUser(userContentItem.As<UserPart>());

                   return fieldValue;

               })
                .Token("Priority", contextParamter =>
                {
                    var c = this.GetTicketPart(context);
                    if (c == null || c.PriorityRecord == null)
                    {
                        return string.Empty;
                    }

                    var records = this.basicDataService.GetPriorities().ToList();
                    return this.GetBasicDataRecordName(c.PriorityRecord.Id, records);

                })
                  .Token("TicketType", contextParamter =>
                {
                    var c = this.GetTicketPart(context);
                    if (c == null)
                    {
                        return string.Empty;
                    }

                    var records = this.basicDataService.GetTicketTypes().ToList();
                    return this.GetBasicDataRecordName(c.Record.TicketType.Id, records);
                })
                 .Token("Number", contextParamter =>
                {
                    var c = this.GetTicketPart(context);
                    if (c == null || c.Record.Identity == null)
                    {
                        return string.Empty;
                    }

                    return c.Record.Identity.Id.ToString(CultureInfo.InvariantCulture);
                })
                 .Token("Title", contextParsmeter =>
                 {
                     var c = this.GetTicketPart(context);
                     if (c == null)
                     {
                         return string.Empty;
                     }

                     return c.Record.Title;
                 })
                 .Token("Id", contextParsmeter =>
                 {
                     var c = this.GetTicketPart(context);
                     if (c == null)
                     {
                         return string.Empty;
                     }

                     return c.Record.Id;
                 });
        }

        private string GetBasicDataRecordName<T>(int? id, IList<T> items)
           where T : IBasicDataRecord
        {
            if (id == null)
            {
                return string.Empty;
            }

            var record = items.FirstOrDefault(c => c.Id == id);

            if (record == null)
            {
                return string.Empty;
            }

            return record.Name;
        }

        private void CheckForContentPartId<TPart>(EvaluateContext context, string contentPartKeyInData)
            where TPart : ContentPart
        {
            if (context.Data.ContainsKey(contentPartKeyInData))
            {
                context.For(contentPartKeyInData, () => context)
                    .Token("Id", contextParamter =>
                    {
                        var part = this.GetPart<TPart>(contextParamter);

                        if (part == null)
                        {
                            return string.Empty;
                        }
                        else
                        {
                            return part.Id;
                        }
                    });
            }
        }
    }
}