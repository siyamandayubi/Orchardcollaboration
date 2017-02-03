using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public interface ISuiteCRMDataService : IDependency
    {
        email_addr_bean_rel GetContact(string email);
        IEnumerable<project> GetProjects(int pageNumber, int pageSize);
        IEnumerable<project> GetProjects(string[] ids);
        int ProjectCount();
    }
}
