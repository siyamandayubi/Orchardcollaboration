using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using S22.IMAP.Models;
using S22.IMAP.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace S22.IMAP.Drivers
{
    /// <summary>
    ///  We define a specific driver instead of using a TemplateFilterForRecord, because we need the model to be the part and not the record.
    /// Thus the encryption/decryption will be done when accessing the part's property
    /// </summary>
    public class IMAPSettingPartDriver : ContentPartDriver<IMAPSettingPart>
    {
        private const string TemplateName = "Parts/IMAPSettings";

        private readonly IIMAPHostRecordService imapHostRecordService;

        public IMAPSettingPartDriver(IIMAPHostRecordService imapHostRecordService)
        {
            this.imapHostRecordService = imapHostRecordService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "IMAPSettings"; } }

        protected override DriverResult Editor(IMAPSettingPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_IMAPSettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix))
                    .OnGroup("IMAP email");
        }

        protected override DriverResult Editor(IMAPSettingPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var previousPassword = part.Password;

            updater.TryUpdateModel(part, Prefix, null, null);
            
            // check whether the form is posted or not
            IsRenderedModel temp = new IsRenderedModel();
            updater.TryUpdateModel(temp, Prefix, null, null);
            if (!temp.IsRendered)
            {
                return null;
            }

            if (!part.IsValid())
            {
                return null;
            }

            part.Host = part.Host.Trim();
            var record = this.imapHostRecordService.Get(part.Host);

            if (record != null && part.EmailsFromMinutesBefore != 0)
            {
                var date = DateTime.UtcNow.AddMinutes(-part.EmailsFromMinutesBefore);
                record = record ?? this.imapHostRecordService.Create(part.Host, 0, date);
                record.EmailUid = 0;
                record.FromDate = date;
                this.imapHostRecordService.Save(record);
            }
            else
            {
                var from = DateTime.UtcNow.AddMinutes(-part.EmailsFromMinutesBefore);
                record = record ?? this.imapHostRecordService.Create(part.Host, 0, from);
                record.FromDate = from;
                this.imapHostRecordService.Save(record);
            }

            part.LastSuccessfullConnectionTime = null;
            part.LatestError = string.Empty;
            part.LatestErrorTime = null;

            return ContentShape("Parts_IMAPSettings_Edit", () =>
            {
                // restore password if the input is empty, meaning it has not been reseted
                if (string.IsNullOrEmpty(part.Password))
                {
                    part.Password = previousPassword;
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("IMAP email");
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