using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMEmailAddressBeanUnitOfWork : SuiteCRMUnitOfWork<email_addr_bean_rel>
    {
        public SuiteCRMEmailAddressBeanUnitOfWork(DbConnection connection) : base(connection) { }
        public SuiteCRMEmailAddressBeanUnitOfWork(ISuiteCRMUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Count()
        {
            return this.context.users.Where(c => c.deleted == 0).Count();
        }

        public IEnumerable<email_addr_bean_rel> GetEmailAddressesBean(string bean, string[] beanIds)
        {
            return this.context.email_addr_bean_rels.Where(c => c.deleted == 0 && c.bean_module == bean && beanIds.Contains(c.bean_id)).ToList();
        }

        public IEnumerable<email_addr_bean_rel> GetBeanIdOfEmailAddress(string bean, string[] emailIds)
        {
            return this.context.email_addr_bean_rels.Where(c => c.deleted == 0 && c.bean_module == bean && emailIds.Contains(c.email_address_id)).ToList();
        }

        public email_addr_bean_rel GetByEmail(string email)
        {
            var query = (from emailAddress in this.context.email_addresses
                         join b in this.context.email_addr_bean_rels on
                             emailAddress.id equals b.email_address_id
                         where emailAddress.email_address.ToLower() == email.ToLower() &&
                         b.bean_module == Helper.ContactsModuleName && b.deleted == 0 && emailAddress.deleted == 0
                         select b
                             );
            return query.FirstOrDefault();
        }
    }
}
