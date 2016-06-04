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

using Orchard.CRM.Core.Services;
using Orchard.Security;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.CRM.Project.ViewModels;
using Orchard.CRM.Core.Models;

namespace Orchard.CRM.Project.Services
{
    public static class ProjectHelper
    {
        public static string GetThumbnailImageOfUser(IUser user)
        {
            if (user == null)
            {
                return string.Empty;
            }

            return GetThumbnailImageOfUser(user.As<UserPart>());
        }

        public static string SplitWithUnvisibleWhitespaces(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                var subStrings = title.Split(' ');

                for (int i = 0; i < subStrings.Length; i++)
                {
                    for (int j = 10; j < subStrings[i].Length; j += 10)
                        subStrings[i] = subStrings[i].Insert(j, "&#8203;");
                }

                title = string.Join(" ", subStrings);
            }

            return title;
        }

        public static string GetThumbnailImageOfUser(UserPart userPart)
        {
            if (userPart == null)
            {
                return string.Empty;
            }

            var user = userPart.ContentItem.Parts.FirstOrDefault(d => d.PartDefinition.Name.ToLower(CultureInfo.InvariantCulture) == "user");
            return CRMHelper.ReteriveField(user, FieldNames.UserThumbnailImageField);
        }

        public static string GetUserField(IUser user, string field)
        {
            if (user == null)
            {
                return string.Empty;
            }

            return GetUserField(user.As<UserPart>(), field);
        }

        public static string GetUserField(UserPart userPart, string field)
        {
            if (userPart == null)
            {
                return string.Empty;
            }

            var user = userPart.ContentItem.Parts.FirstOrDefault(d => d.PartDefinition.Name.ToLower(CultureInfo.InvariantCulture) == "user");
            string value = CRMHelper.ReteriveField(user, field);

            return value;
        }

        public static T Convert<T>(IUser user)
            where T : UserViewModel, new()
        {
            UserPart userPart = user.As<UserPart>();

            var userModel = new T
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Fullname = CRMHelper.GetFullNameOfUser(user),
                Thumbnail = ProjectHelper.GetThumbnailImageOfUser(user),
                SkypeId = GetUserField(userPart, FieldNames.UserSkypeIdField),
                Tel = GetUserField(userPart, FieldNames.UserTelField),
                Tags = GetUserField(userPart, FieldNames.UserTags),
                Mobile = GetUserField(userPart, FieldNames.UserMobileField)
            };

            return userModel;
        }

        public static int? GetProjectId(ContentPart part, IProjectService projectService)
        {
            int? projectId = null;

            AttachToProjectPart attachToProjectPart = part.As<AttachToProjectPart>();
            if (attachToProjectPart == null || attachToProjectPart.Record.Project == null)
            {
                projectId = projectService.GetProjectIdFromQueryString();

                if (projectId == null)
                {
                    return null;
                }
            }
            else
            {
                projectId = attachToProjectPart.Record.Project.Id;
            }

            return projectId;
        }
    }
}