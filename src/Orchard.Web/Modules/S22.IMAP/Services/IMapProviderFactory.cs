using Orchard;
using S22.Imap.Provider;
using S22.IMAP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace S22.IMAP.Services
{
    public class IMapProviderFactory : IIMapProviderFactory
    {
        protected readonly IOrchardServices orchardServices;

        public IMapProviderFactory(IOrchardServices orchardServices)
        {
            this.orchardServices = orchardServices;
        }

        public IImapClient Create()
        {
            var imapSetting = this.orchardServices.WorkContext.CurrentSite.As<IMAPSettingPart>();

            if (!imapSetting.IsValid())
            {
                return null;
            }

            var client = new ImapClient(hostname: imapSetting.Host, port: imapSetting.Port, ssl: imapSetting.EnableSsl, username: imapSetting.Username, password: imapSetting.Password);

            if (!string.IsNullOrEmpty(imapSetting.DefaultBox))
            {
                client.DefaultMailbox = imapSetting.DefaultBox;
            }

            return client;
        }
    }
}