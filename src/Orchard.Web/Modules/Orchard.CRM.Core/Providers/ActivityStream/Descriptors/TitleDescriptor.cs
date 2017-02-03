using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.Localization;
using System.Globalization;
using Orchard.Core.Title.Models;

namespace Orchard.CRM.Core.Providers.ActivityStream.Descriptors
{
    public class TitleDescriptor : IContentItemDescriptor
    {
        public string GetDescription(IContent content)
        {
            var titlePart = content.As<TitlePart>();
            if (titlePart == null)
            {
                return string.Empty;
            }

            return titlePart.Title;
        }

        public bool CanApply(IContent content)
        {
            return content.As<TitlePart>() != null;
        }
    }
}