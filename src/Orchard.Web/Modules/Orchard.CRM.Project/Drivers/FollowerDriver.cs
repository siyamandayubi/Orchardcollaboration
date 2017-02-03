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
            List<DriverResult> shapes = new List<DriverResult>();

            shapes.Add(ContentShape("Parts_Followers", () => shapeHelper.Parts_Followers(
                contentItem: part.ContentItem,
                Count: string.IsNullOrEmpty(part.Followers) ? 0 : part.Followers.Split(',').Length)));

            if (services.WorkContext.CurrentUser != null)
            {
                if (displayType == "Detail")
                {
                    string currentUserId = services.WorkContext.CurrentUser.Id.ToString(CultureInfo.InvariantCulture);

                    string followers = part.Followers;
                    followers = followers ?? string.Empty;

                    bool followed = followers.Contains(currentUserId);

                    dynamic model = new ExpandoObject();
                    model.ContentItemId = part.ContentItem.Id;
                    model.Followed = followed;

                    shapes.Add(ContentShape("Parts_Follow_Link", () => shapeHelper.FollowLink(contentItem: part.ContentItem)));
                }
            }

            return Combined(shapes.ToArray());
        }
    }
}