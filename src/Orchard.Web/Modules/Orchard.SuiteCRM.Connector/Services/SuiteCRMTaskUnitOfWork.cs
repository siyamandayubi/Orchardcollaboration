using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMTaskUnitOfWork : SuiteCRMUnitOfWork<task>
    {
        public SuiteCRMTaskUnitOfWork(DbConnection connection) : base(connection) { }
        public SuiteCRMTaskUnitOfWork(ISuiteCRMUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<task> GetTasks(string[] ids)
        {
            return this.context.tasks.Where(c => c.deleted == 0 && ids.Contains(c.id)).ToList();
        }
    }
}
