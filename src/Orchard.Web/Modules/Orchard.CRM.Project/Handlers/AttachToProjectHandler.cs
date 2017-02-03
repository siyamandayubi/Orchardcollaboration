using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Indexing;
using Orchard.CRM.Core.Controllers;

namespace Orchard.CRM.Project.Handlers
{
    public class AttachToProjectHandler : ContentHandler
    {
        public AttachToProjectHandler(IIndexProvider indexProvider, IContentManager contentManager)
        {

        }
    }
}