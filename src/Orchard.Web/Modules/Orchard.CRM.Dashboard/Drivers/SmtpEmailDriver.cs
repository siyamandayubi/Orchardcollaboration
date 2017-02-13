using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.CRM.Dashboard.Models;
using Orchard.Email.Models;
using Orchard.ContentManagement;
using Newtonsoft.Json.Linq;

namespace Orchard.CRM.Dashboard.Drivers
{
    public class SmtpEmailDriver : ContentPartDriver<SmtpEmailPortletPart>
    {
        protected readonly IOrchardServices orchardServices;

        public SmtpEmailDriver(IOrchardServices orchardServices)
        {
            this.orchardServices = orchardServices;
        }

        protected override DriverResult Display(SmtpEmailPortletPart part, string displayType, dynamic shapeHelper)
        {
            var smtpSettings = orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            dynamic model = new JObject();
            model.IsConfigured = smtpSettings.IsValid();
            model.Host = smtpSettings.Host;

            return ContentShape("Parts_SmtpEmailPortlet_Summary", () => shapeHelper.Parts_SmtpEmailPortlet_Summary(Model: model));
        }
    }
}