using Orchard;
using S22.Imap.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace S22.IMAP.Services
{
    public interface IIMapProviderFactory: IDependency
    {
        IImapClient Create();
    }
}