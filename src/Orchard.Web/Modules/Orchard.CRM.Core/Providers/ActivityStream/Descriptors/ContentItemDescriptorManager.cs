using Orchard.ContentManagement;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.ActivityStream.Descriptors
{
    public class ContentItemDescriptorManager : IContentItemDescriptorManager
    {
        private readonly IEnumerable<IContentItemDescriptor> descriptors;
        
        public ContentItemDescriptorManager(IEnumerable<IContentItemDescriptor> descriptors)
        {
            this.descriptors = descriptors;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string GetDescription(IContent content)
        {
            foreach (var descriptor in this.descriptors)
            {
                if (descriptor.CanApply(content))
                {
                    return descriptor.GetDescription(content);
                }
            }

            return this.T("Content - {0}", content.Id.ToString(CultureInfo.InvariantCulture)).Text;
        }
    }
}