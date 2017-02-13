using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Handlers
{
    public class ProjectHandler : ContentHandler
    {
        public ProjectHandler(IRepository<ProjectPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}