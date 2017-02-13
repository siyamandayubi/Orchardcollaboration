using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMEmailAddressUnitOfWork : SuiteCRMUnitOfWork<email_addresses>
    {
        public SuiteCRMEmailAddressUnitOfWork(DbConnection connection) : base(connection) { }
        public SuiteCRMEmailAddressUnitOfWork(ISuiteCRMUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Count()
        {
            return this.context.users.Where(c => c.deleted == 0).Count();
        }

        public IEnumerable<email_addresses> GetEmailAddresses(string[] ids)
        {
            return this.context.email_addresses.Where(c => c.deleted == 0 && ids.Contains(c.id)).ToList();
        }

        public email_addresses GetByEmail(string email)
        {
            return this.context.email_addresses.FirstOrDefault(c => c.deleted == 0 && c.email_address.ToLower() == email.ToLower());
        }
    }
}
