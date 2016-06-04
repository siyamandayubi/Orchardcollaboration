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