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

using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Drivers
{
    public abstract class CRMContentPartDriver<TContent> : ContentPartDriver<TContent>
        where TContent : ContentPart, new()
    {
        protected IOrchardServices services;
        
        public CRMContentPartDriver(IOrchardServices services)
        {
            this.services = services;
        }

        public static string GetViewName(string displayType, string name)
        {
            return string.Format(CultureInfo.InstalledUICulture, "Parts_{0}{1}", displayType, name);
        }

        protected string GetTitle(IUpdateModel updater)
        {
            TempTitle temp = new TempTitle();
            updater.TryUpdateModel(temp, "TitlePart", null, null);

            return temp.Title;
        }

        protected int? GetPropertyFromRequest(string fieldName)
        {
            var queryString = this.services.WorkContext.HttpContext.Request.QueryString;
            var form = this.services.WorkContext.HttpContext.Request.Form;

            if (queryString.AllKeys.Contains(fieldName))
            {
                return int.Parse(queryString[fieldName]);
            }

            if (form.AllKeys.Contains(fieldName))
            {
                return int.Parse(form[fieldName]);
            }

            return null;
        }

        private class TempTitle
        {
            public string Title { get; set; }
        }
    }
}