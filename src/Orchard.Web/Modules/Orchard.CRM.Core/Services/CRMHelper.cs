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

namespace Orchard.CRM.Core.Services
{
    using Newtonsoft.Json.Linq;
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.FieldStorage.InfosetStorage;
    using Orchard.Core.Title.Models;
    using Orchard.CRM.Core.Models;
    using Orchard.Localization;
    using Orchard.Security;
    using Orchard.Users.Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    public static class CRMHelper
    {
        public static readonly string OrchardCollaborationEmailMessageType = "OrchardCollaboration.Email";
        public static readonly string OrcharCollaborationDefinitiveEmailMessageType = "DefinitiveOrchardCollaboration.Email";
        
        public static void AddStatusGroupRecordsToModel(IEnumerable<StatusRecord> statusRecords, Collection<KeyValuePair<int?, int>> groups, Collection<dynamic> model)
        {
            foreach (var statusRecord in statusRecords)
            {
                dynamic item = new JObject();
                item.Id = statusRecord.Id;
                item.Name = statusRecord.Name;
                item.OrderId = statusRecord.OrderId;
                var groupCount = groups.FirstOrDefault(c => c.Key == statusRecord.Id);
                item.Count = groupCount.Key != null ? groupCount.Value : 0;
                model.Add(item);
            }

            // add entry for null items
            if (groups.Count(c => c.Key == null) > 0)
            {
                var currentUserTicketsWithoutStatus = groups.FirstOrDefault(c => c.Key == null);
                dynamic item = new JObject();
                item.Name = "No Status";
                item.Id = null;
                item.OrderId = 0;
                item.Count = currentUserTicketsWithoutStatus.Value;
                model.Add(item);
            }
        }

        public static string GetContentItemTitle(ContentItem contentItem)
        {
            TicketPart ticketPart = contentItem.As<TicketPart>();
            if (ticketPart != null)
            {
                string title = ticketPart.Record.Title;
                string identity = ticketPart.Record.Identity != null ?
                    ticketPart.Record.Identity.Id.ToString(CultureInfo.InvariantCulture) :
                    string.Empty;

                return string.Format(CultureInfo.CurrentUICulture, "{0} - {1}", identity, title);
            }

            TitlePart titlePart = contentItem.As<TitlePart>();
            if (titlePart != null)
            {
                return titlePart.Title;
            }

            return contentItem.ContentType + " " + contentItem.Id.ToString(CultureInfo.InvariantCulture);
        }

        public static string GetFullNameOfUser(IUser user)
        {
            if (user == null)
            {
                return string.Empty;
            }

            return GetFullNameOfUser(user.As<UserPart>());
        }

        public static DateTime SetSiteTimeZone(WorkContext workContext, DateTime dateTime)
        {
            var currentSite = workContext.CurrentSite;

            if (String.IsNullOrEmpty(currentSite.SiteTimeZone))
            {
                return dateTime;
            }

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, TimeZoneInfo.Utc.Id, currentSite.SiteTimeZone);
        }

        public static string GetFullNameOfUser(UserPart userPart)
        {
            if (userPart == null)
            {
                return string.Empty;
            }

            var user = userPart.ContentItem.Parts.FirstOrDefault(d => d.PartDefinition.Name.ToLower(CultureInfo.InvariantCulture) == "user");
            string fullName = CRMHelper.ReteriveField(user, "FullName");

            return string.IsNullOrEmpty(fullName) ? userPart.UserName : fullName;
        }

        public static string ReteriveField(ContentPart part, string name)
        {
            var infosetPart = part.As<InfosetPart>();
            var mainElement = infosetPart.VersionInfoset.Element;
            var partElement = mainElement.Element(part.PartDefinition.Name);
            if (partElement == null)
            {
                return string.Empty;
            }

            var field = partElement.Element(name);

            if (field == null)
            {
                return string.Empty;
            }

            return field.FirstNode != null ? field.FirstNode.ToString() : string.Empty;
        }

        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            // assign the model of the controller from which this method was called to the instance of the passed controller (a new instance, by the way)
            controller.ViewData.Model = model;

            // initialize a string builder
            using (StringWriter sw = new StringWriter())
            {
                // find and load the view or partial view, pass it through the controller factory
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);

                // render it
                viewResult.View.Render(viewContext, sw);

                //return the razorized view/partial-view as a string
                return sw.ToString();
            }
        }

        public static DateTime SetSiteTimeZone(IOrchardServices services, DateTime dateTime)
        {
            var currentSite = services.WorkContext.CurrentSite;

            if (String.IsNullOrEmpty(currentSite.SiteTimeZone))
            {
                return dateTime;
            }

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, TimeZoneInfo.Utc.Id, currentSite.SiteTimeZone);
        }

        public static Collection<SelectListItem> GetDueDateItems(Localizer localizer)
        {
            Collection<SelectListItem> dueDates = new Collection<SelectListItem>();

            for (int i = 1; i <= 6; i++)
            {
                string text = string.Format(CultureInfo.InvariantCulture, "{0} day{1} later", i.ToString(CultureInfo.InvariantCulture), i > 0 ? "s" : string.Empty);
                dueDates.Add(new SelectListItem { Text = localizer(text).ToString(), Value = i.ToString(CultureInfo.InvariantCulture) });
            }

            dueDates.Add(new SelectListItem { Text = localizer("one week later").ToString(), Value = "7" });
            dueDates.Add(new SelectListItem { Text = localizer("two weeks later").ToString(), Value = "14" });
            dueDates.Add(new SelectListItem { Text = localizer("four weeks later").ToString(), Value = "28" });
            dueDates.Insert(0, new SelectListItem());

            return dueDates;
        }
    }
}