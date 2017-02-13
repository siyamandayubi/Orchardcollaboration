using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.CRM.Dashboard.Models;
using S22.IMAP.Models;
using Orchard.ContentManagement;
using Newtonsoft.Json.Linq;

namespace Orchard.CRM.Dashboard.Drivers
{
    public class ImapEmailDriver : ContentPartDriver<IMapEmailPortletPart>
    {
        protected readonly IOrchardServices orchardServices;

        public ImapEmailDriver(IOrchardServices orchardServices)
        {
            this.orchardServices = orchardServices;
        }

        protected override DriverResult Display(IMapEmailPortletPart part, string displayType, dynamic shapeHelper)
        {
            var imapSetting = this.orchardServices.WorkContext.CurrentSite.As<IMAPSettingPart>();
            dynamic model = new JObject();
            model.IsConfigured = imapSetting.IsValid();
            model.LastSuccessfulConnection = imapSetting.LastSuccessfullConnectionTime;
            model.LatestError = imapSetting.LatestError;
            model.LatestErrorTime = imapSetting.LatestErrorTime;
            model.Host = imapSetting.Host;

            return ContentShape("Parts_IMapEmailPortlet_Summary", () => shapeHelper.Parts_IMapEmailPortlet_Summary(Model: model));
        }
    }
}