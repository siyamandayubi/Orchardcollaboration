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

using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Project.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Drivers
{
    public class FollowerDriver : ContentPartDriver<FollowerPart>
    {
        private readonly IOrchardServices services;

        public FollowerDriver(IOrchardServices services)
        {
            this.services = services;
        }

        protected override DriverResult Display(FollowerPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType != "Detail" || services.WorkContext.CurrentUser == null)
            {
                return null;
            }

            string currentUserId = services.WorkContext.CurrentUser.Id.ToString(CultureInfo.InvariantCulture);

            string followers = part.Followers;
            followers = followers ?? string.Empty;

            bool followed = followers.Contains(currentUserId);

            dynamic model = new ExpandoObject();
            model.ContentItemId = part.ContentItem.Id;
            model.Followed = followed;

            return ContentShape("Parts_Follow_Link",   () => shapeHelper.FollowLink(contentItem: part.ContentItem));
        }
    }
}