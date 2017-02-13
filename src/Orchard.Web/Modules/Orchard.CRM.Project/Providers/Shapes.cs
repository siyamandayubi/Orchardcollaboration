using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.Providers
{
    public class Shapes : IDependency
    {
        private readonly IBasicDataService basicDataService;
        private readonly ICRMContentOwnershipService contentOwnershipService;
        private readonly IOrchardServices services;

        public Shapes(IBasicDataService basicDataService, ICRMContentOwnershipService contentOwnershipService, IOrchardServices services)
        {
            this.services = services;
            this.contentOwnershipService = contentOwnershipService;
            this.basicDataService = basicDataService;
        }

        [Shape]
        public void ItemMenu(dynamic shape, dynamic Display, TextWriter Output, ContentItem contentItem)
        {
            bool canEdit = this.contentOwnershipService.CurrentUserCanEditContent(contentItem);
            bool canChangePermission = this.contentOwnershipService.CurrentUserCanChangePermission(contentItem);
            bool isAdvancedOperator = this.contentOwnershipService.IsCurrentUserAdvanceOperator();

            AttachToProjectPart attachToProjectPart = contentItem.As<AttachToProjectPart>();
            if (attachToProjectPart != null && attachToProjectPart.Record.Project != null)
            {
                Output.Write(Display.Item_MenuView(CanEdit: canEdit, CanChangePermission: canChangePermission, IsAdvancedOperator: isAdvancedOperator, ContentItem: contentItem));
            }
        }

        [Shape]
        public void FollowLink(dynamic shape, dynamic Display, TextWriter Output, ContentItem contentItem)
        {
            if (services.WorkContext.CurrentUser == null)
            {
                return;
            }

            var part = contentItem.As<FollowerPart>();

            if (part == null)
            {
                return;
            }

            string currentUserId = services.WorkContext.CurrentUser.Id.ToString(CultureInfo.InvariantCulture);

            string followers = part.Followers;
            followers = followers ?? string.Empty;

            bool followed = followers.Contains(currentUserId);

            Output.Write(Display.FollowerLinkView(ContentItemId: part.ContentItem.Id, Followed: followed, ContentType: contentItem.ContentType));
        }

        [Shape]
        public void UserThumbnail(
            dynamic Shape,
            dynamic Display,
            TextWriter Output,
            int userId,
            string source,
            int width = 48,
            int height = 48,
            string cssClasses = "",
            string userProfileUrl = "")
        {
            var user = this.basicDataService.GetOperatorOrCustomerUser(userId);
            var imageTag = new TagBuilder("img");
            imageTag.Attributes.Add("width", width.ToString(CultureInfo.InvariantCulture));
            imageTag.Attributes.Add("height", height.ToString(CultureInfo.InvariantCulture));
            string fullName = CRMHelper.GetFullNameOfUser(user);
            imageTag.Attributes.Add("title", fullName);

            if (string.IsNullOrEmpty(cssClasses))
            {
                imageTag.Attributes.Add("class", cssClasses);
            }

            if (user != null)
            {
                string temp = ProjectHelper.GetThumbnailImageOfUser(user);

                if (!string.IsNullOrEmpty(temp))
                {
                    source = temp;
                }
            }

            imageTag.Attributes.Add("src", source);

            TagBuilder linkTag = null;

            if (!string.IsNullOrEmpty(userProfileUrl))
            {
                linkTag = new TagBuilder("a");
                linkTag.Attributes.Add("href", userProfileUrl);
                Output.Write(linkTag.ToString(TagRenderMode.StartTag));
            }

            Output.Write(imageTag.ToString(TagRenderMode.StartTag));
            Output.Write(imageTag.ToString(TagRenderMode.EndTag));


            if (!string.IsNullOrEmpty(userProfileUrl))
            {
                Output.Write(linkTag.ToString(TagRenderMode.EndTag));
            }
        }
    }
}