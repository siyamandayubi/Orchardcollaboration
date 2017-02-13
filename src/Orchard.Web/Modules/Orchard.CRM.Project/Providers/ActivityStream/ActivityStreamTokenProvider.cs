using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using Orchard.CRM.Core.Services;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.CRM.Project.Providers.ActivityStream
{
    public class ActivityStreamTokenProvider : Orchard.Tokens.ITokenProvider
    {
        private Localizer T { get; set; }
        private readonly IContentManager contentManager;
        private readonly UrlHelper urlHelper;
        private readonly IActivityStreamService activityStreamService;
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;

        public const string ActivityStreamRecordKey = "EActivityStream";

        public ActivityStreamTokenProvider(IContentManager contentManager, IActivityStreamService activityStreamService, UrlHelper urlHelper, IContentItemDescriptorManager contentItemDescriptorManager)
        {
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.urlHelper = urlHelper;
            this.activityStreamService = activityStreamService;
            this.contentManager = contentManager;
            this.T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context)
        {
            context.For(ActivityStreamTokenProvider.ActivityStreamRecordKey, T("ActivityStream Item"), T("ActivityStream Item"))
                 .Token("ChangeDescription", T("ChangeDescription"), T("ChangeDescription"))
                 .Token("ContentDescription", T("ContentDescription"), T("ContentDescription"))
                 .Token("Url", T("Url"), T("Url"))
                 .Token("Changes", T("Changes"), T("Changes"))
                 .Token("ChangedByUsername", T("ChangedByUsername"), T("ChangedByUsername"))
                 .Token("ChangedByFullname", T("ChangedByFullname"), T("ChangedByFullname"));
        }

        public void Evaluate(EvaluateContext context)
        {
            context.For(ActivityStreamTokenProvider.ActivityStreamRecordKey, () => this.GetActivityStream(context))
                .Token("ContentDescription", c =>
                {
                    ContentItem contentItem = (ContentItem)context.Data["Content"];
                    if (contentItem == null)
                    {
                        return null;
                    }

                    return this.contentItemDescriptorManager.GetDescription(contentItem);
                })
                .Token("ChangeDescription", c =>
                {
                    if (c == null)
                    {
                        return string.Empty;
                    }

                    dynamic model = this.activityStreamService.CreateModel(c);
                    return model.ContentDescription;
                })
               .Token("ChangedByFullname", c =>
               {
                   if (c == null)
                   {
                       return string.Empty;
                   }

                   dynamic model = this.activityStreamService.CreateModel(c);
                   return model.UserFullName;
               })
             .Token("Url", c =>
             {
                 if (c == null)
                 {
                     return string.Empty;
                 }

                 dynamic model = this.activityStreamService.CreateModel(c);
                 RouteValueDictionary route = model.Route;

                 if (route == null)
                 {
                     return string.Empty;
                 }

                 string action = route.ContainsKey("action") ? route["action"].ToString() : string.Empty;

                 Uri requestUrl = urlHelper.RequestContext.HttpContext.Request.Url;
                 var virtualPath = urlHelper.Action(action, route);
                 var result = string.Format("{0}://{1}{2}",
                                        requestUrl.Scheme,
                                        requestUrl.Authority,
                                        VirtualPathUtility.ToAbsolute(virtualPath));

                 return result;
             })
               .Token("ChangedByUsername", c =>
               {
                   if (c == null)
                   {
                       return string.Empty;
                   }

                   dynamic model = this.activityStreamService.CreateModel(c);
                   return model.User.UserName;
               })
                .Token("Changes", c =>
                {
                    if (c == null)
                    {
                        return string.Empty;
                    }

                    dynamic model = this.activityStreamService.CreateModel(c);
                    string[] changes = model.Changes.ToArray();

                    return string.Join("<br></br>", changes);
                });
        }

        private ActivityStreamRecord GetActivityStream(EvaluateContext context)
        {
            return (ActivityStreamRecord)context.Data[ActivityStreamRecordKey];
        }
    }
}