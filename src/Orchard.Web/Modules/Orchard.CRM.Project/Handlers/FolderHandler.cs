using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Project.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;

namespace Orchard.CRM.Project.Handlers
{
    public class FolderHandler : ContentHandler
    {
        public FolderHandler(IRepository<FolderPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));

            // Set the project of AttachToProjectPart
            this.OnPublishing<FolderPart>((context, part) =>
            {
                var attachToProjectPart = part.As<AttachToProjectPart>();
                part.Record.Project = attachToProjectPart.Record.Project;
            });
        }
    }
}