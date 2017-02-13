using Orchard.SuiteCRM.Connector.ViewModels;
using System.Collections.Generic;

namespace Orchard.SuiteCRM.Connector.Services
{
    public interface ISuiteCRMSyncUserService : IDependency
    {
        int GetSuiteCRMUsersCount();
        IEnumerable<SuiteCRMUserViewModel> CopySuiteCRMUsersToOrchard(CopySuiteCRMUsersToOrchardViewModel model);
        IEnumerable<SuiteCRMUserViewModel> GetUsers(int pageNumber, int pageSize);
    }
}
