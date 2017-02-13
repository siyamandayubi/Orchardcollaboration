using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.SuiteCRM.Connector.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Drivers
{
    public class SuiteCRMSettingPartDriver : ContentPartDriver<SuiteCRMSettingPart>
    {
        private const string TemplateName = "Parts/SuiteCRMSettingPart";

        public SuiteCRMSettingPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "SuiteCRMSettingPart"; } }

        protected override DriverResult Editor(SuiteCRMSettingPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_SuiteCRMSettingPart_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix))
                    .OnGroup("SuiteCRM");
        }

        protected override DriverResult Editor(SuiteCRMSettingPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var previousPassword = part.Password;

            updater.TryUpdateModel(part, Prefix, null, null);

            return ContentShape("Parts_SuiteCRMSettingPart_Edit", () =>
            {
                // restore password if the input is empty, meaning it has not been reseted
                if (string.IsNullOrEmpty(part.Password))
                {
                    part.Password = previousPassword;
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("SuiteCRM");
        }

        /// <summary>
        /// The class is used to check whether the part is posted or not, because by visiting any
        /// Part of the SiteSetting, Orchard tries to update all of the Parts of the SiteSetting
        /// </summary>
        private class IsRenderedModel
        {
            public bool IsRendered { get; set; }
        }
    }
}