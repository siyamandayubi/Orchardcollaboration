using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.CRM.Core.Services
{
    public interface ICRMContentOwnershipServiceExtension : IDependency
    {
        bool HasAccessTo(IContent content, ICRMContentOwnershipService contentOwnershipService);
        bool CanApply(IContent content, ICRMContentOwnershipService contentOwnershipService);
    }
}
