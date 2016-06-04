using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using S22.IMAP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace S22.IMAP.Handlers
{
    public class IMapEmailHandler : ContentHandler
    {
        public IMapEmailHandler(IRepository<IMAPEmailPart> repository)
        {
            Filters.Add(new ActivatingFilter<IMAPEmailPart>(IMAPEmailPart.ContentItemTypeName));
        }
    }
}