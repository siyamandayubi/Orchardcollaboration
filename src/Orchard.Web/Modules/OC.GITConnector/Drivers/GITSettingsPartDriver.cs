using OC.GITConnector.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Linq;

namespace OC.GITConnector.Drivers
{
    /// <summary>
    ///  We define a specific driver instead of using a TemplateFilterForRecord, because we need the model to be the part and not the record.
    /// Thus the encryption/decryption will be done when accessing the part's property
    /// </summary>
    public class GITSettingsPartDriver : ContentPartDriver<GITSettingsPart>
    {
        private const string TemplateName = "Parts/GITSettings";

        private readonly IRepository<GITServerRecord> svnServerRepository;

        public GITSettingsPartDriver(IRepository<GITServerRecord> svnServerRepository)
        {
            this.svnServerRepository = svnServerRepository;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "GITSettings"; } }

        protected override DriverResult Editor(GITSettingsPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_GITSettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix))
                    .OnGroup("GIT Client");
        }

        protected override DriverResult Editor(GITSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
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

            part.Server = part.Server.Trim();
            var record = this.svnServerRepository.Table.FirstOrDefault(c => c.Server == part.Server);

            if (record == null)
            {
                record = new GITServerRecord { Server = part.Server, LastRevision = part.LastRevision, FromDate = DateTime.UtcNow };
                this.svnServerRepository.Create(record);
            }
            else
            {
                record.LastRevision = part.LastRevision;
            }

            part.LastSuccessfullConnectionTime = null;
            part.LatestError = string.Empty;
            part.LatestErrorTime = null;

            return ContentShape("Parts_GITSettings_Edit", () =>
            {
                // restore password if the input is empty, meaning it has not been reseted
                if (string.IsNullOrEmpty(part.Password))
                {
                    part.Password = previousPassword;
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("GIT Client");
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