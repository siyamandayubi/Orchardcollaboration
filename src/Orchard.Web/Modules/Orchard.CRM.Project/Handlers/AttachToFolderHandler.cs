using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Project.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Handlers
{
    public class AttachToFolderHandler : ContentHandler
    {
        public AttachToFolderHandler(IRepository<AttachToFolderPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}