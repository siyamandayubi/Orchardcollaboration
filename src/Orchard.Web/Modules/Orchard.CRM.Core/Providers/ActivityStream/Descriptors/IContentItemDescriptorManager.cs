using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.ActivityStream.Descriptors
{
    public interface IContentItemDescriptorManager : IDependency
    {
        string GetDescription(IContent content);
    }
}