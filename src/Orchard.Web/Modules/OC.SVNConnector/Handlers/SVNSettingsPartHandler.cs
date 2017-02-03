
namespace OC.SVNConnector.Handlers
{
    using Models;
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Handlers;
    using Orchard.Localization;
    using Orchard.Logging;
    using Orchard.Security;
    using System;
    using System.Text;

    public class SVNSettingsPartHandler : ContentHandler
    {
        private readonly IEncryptionService _encryptionService;

        public SVNSettingsPartHandler(IEncryptionService encryptionService)
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            _encryptionService = encryptionService;
            Filters.Add(new ActivatingFilter<SVNSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<SVNSettingsPart>("SVNSettingsPart", "Parts/SVNSettings", "SVN Client"));

            OnLoaded<SVNSettingsPart>(LazyLoadHandlers);
        }

        public new ILogger Logger { get; set; }

        public Localizer T { get; set; }

        private void LazyLoadHandlers(LoadContentContext context, SVNSettingsPart part)
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
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("SVN Client")));
        }
    }
}