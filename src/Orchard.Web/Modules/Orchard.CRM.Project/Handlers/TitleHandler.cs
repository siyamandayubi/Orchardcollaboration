using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Logging;

namespace Orchard.CRM.Project.Handlers
{
    public class TitleHandler : ContentHandler
    {
        public TitleHandler()
        {
            OnIndexing<TitlePart>((context, part) =>
            {
                AttachToProjectPart attachToProjectPart = part.As<AttachToProjectPart>();

                if (attachToProjectPart == null)
                {
                    return;
                }

                try
                {
                    string title = part.Title ?? string.Empty;
                    context.DocumentIndex.Add(FieldNames.TitleFieldName, title).Analyze().Store();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }
    }
}