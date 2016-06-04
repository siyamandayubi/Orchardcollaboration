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
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.SuiteCRM.Connector.Models;
using System;
using System.Text;

namespace Orchard.SuiteCRM.Connector.Handlers
{
    public class SuiteCRMHandler: ContentHandler
    {
        private readonly IEncryptionService _encryptionService;

        public SuiteCRMHandler(IEncryptionService encryptionService)
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            _encryptionService = encryptionService;
            Filters.Add(new ActivatingFilter<SuiteCRMSettingPart>("Site"));
            Filters.Add(new TemplateFilterForPart<SuiteCRMSettingPart>("SuiteCRMSettingPart", "Parts/SuiteCRMSettingPart", "SuiteCRM Settings"));

            OnLoaded<SuiteCRMSettingPart>(LazyLoadHandlers);
        }

        public new ILogger Logger { get; set; }

        public Localizer T { get; set; }

        private void LazyLoadHandlers(LoadContentContext context, SuiteCRMSettingPart part)
        {
            part.PasswordField.Getter(() =>
            {
                try
                {
                    var encryptedPassword = part.Retrieve(x => x.Password);
                    return String.IsNullOrWhiteSpace(encryptedPassword) ? String.Empty : Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(encryptedPassword)));
                }
                catch
                {
                    Logger.Error("The password could not be decrypted. It might be corrupted, try to reset it.");
                    return null;
                }
            });

            part.PasswordField.Setter(value =>
            {
                var encryptedPassword = String.IsNullOrWhiteSpace(value) ? String.Empty : Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(value)));
                part.Store(x => x.Password, encryptedPassword);
            });
        }


        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("SuiteCRM Settings")));
        }
    }
}