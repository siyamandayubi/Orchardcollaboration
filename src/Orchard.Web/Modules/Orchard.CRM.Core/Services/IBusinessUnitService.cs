using Orchard.CRM.Core.ViewModels;
using System;
using System.Collections.ObjectModel;
namespace Orchard.CRM.Core.Services
{
    public interface IBusinessUnitService : IDependency
    {
        void Fill(Collection<BusinessUnitViewModel> target, bool restrictToUserPermissions);
    }
}
