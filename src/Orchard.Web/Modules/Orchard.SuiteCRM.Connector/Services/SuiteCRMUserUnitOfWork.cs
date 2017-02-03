using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMUserUnitOfWork : SuiteCRMUnitOfWork<user>
    {
        public SuiteCRMUserUnitOfWork(DbConnection connection) : base(connection) { }
        public SuiteCRMUserUnitOfWork(ISuiteCRMUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<user> GetUsers(int pageNumber, int pageSize)
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
                .users
                .Where(c => c.deleted == 0 && c.user_name != null && c.status == "Active")
                .OrderByDescending(c=>c.date_entered)
                .Skip(pageSize * pageNumber)
                .Take(pageSize)
                .ToList();
        }

        public int Count()
        {
            return this.context.users.Where(c => c.deleted == 0).Count();
        }

        public IEnumerable<user> GetUsers(string[] ids)
        {
            return this.context
                .users
                .Where(c => c.deleted == 0 && c.user_name != null && c.status == "Active" && ids.Contains(c.id))
                .ToList();
        }
    }
}
