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