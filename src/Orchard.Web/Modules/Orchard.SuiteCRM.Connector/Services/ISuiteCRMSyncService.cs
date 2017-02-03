using Orchard.SuiteCRM.Connector.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public interface ISuiteCRMSyncService : IDependency
    {
        IEnumerable<SuiteCRMTaskDetailViewModel> CopyOrchardTicketsToSuite(CopyOrchardTasksToSuiteViewModel model);
        IEnumerable<SuiteCRMProjectDetailViewModel> GetProjects(int pageNumber, int pageSize, bool basedOnSuiteCRMList);
        IEnumerable<SuiteCRMProjectDetailViewModel> CopyOrchardProjectsToSuite(CopyOrchardProjectToSuiteViewModel model);
        IEnumerable<SuiteCRMProjectDetailViewModel> CopySuiteCRMProjectsToOrchard(CopyOrchardProjectToSuiteViewModel model);
    }
}
