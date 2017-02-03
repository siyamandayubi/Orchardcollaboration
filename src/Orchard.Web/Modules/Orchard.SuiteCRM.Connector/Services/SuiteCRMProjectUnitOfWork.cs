using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMProjectUnitOfWork : SuiteCRMUnitOfWork<project>
    {
        public SuiteCRMProjectUnitOfWork(DbConnection connection) : base(connection) { }
        public SuiteCRMProjectUnitOfWork(ISuiteCRMUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<project> GetProjects(int pageNumber, int pageSize)
        {
            if (pageSize == 0)
            {
                pageSize = 20;
            }

            if (pageNumber < 0)
            {
                pageNumber = 0;
            }

            return this.context
                .projects
                .Where(c => c.deleted == 0)
                .OrderByDescending(c=>c.date_entered)
                .Skip(pageSize * pageNumber)
                .Take(pageSize)
                .ToList();
        }

        public int Count()
        {
            return this.context.projects.Where(c => c.deleted == 0).Count();
        }

        public IEnumerable<project> GetProjects(string[] ids)
        {
            return this.context.projects.Where(c => c.deleted == 0 && ids.Contains(c.id)).ToList();
        }
    }
}
