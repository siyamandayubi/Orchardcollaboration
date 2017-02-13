using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMProjectTaskUnitOfWork : SuiteCRMUnitOfWork<project_task>
    {
        public SuiteCRMProjectTaskUnitOfWork(DbConnection connection) : base(connection) { }
        public SuiteCRMProjectTaskUnitOfWork(ISuiteCRMUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<project_task> GetProjectTasks(string projectId)
        {
            return this.context.project_tasks.Where(c => c.deleted == 0 && c.project_id == projectId).ToList();
        }

        public IEnumerable<project_task> GetTasks(string[] ids)
        {
            return this.context.project_tasks.Where(c => c.deleted == 0 && ids.Contains(c.id)).ToList();
        }
    }
}
