using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Orchard.CRM.Project.Handlers
{
    public class BodyHandler : ContentHandler
    {
        public BodyHandler()
        {
            OnIndexing<BodyPart>((context, part) =>
            {
                AttachToProjectPart attachToProjectPart = part.As<AttachToProjectPart>();

                if (attachToProjectPart == null)
                {
                    return;
                }

                string text = part.Text ?? string.Empty;
                context.DocumentIndex.Add(FieldNames.BodyFieldName, text).Analyze().Store();
            });
        }
    }
}