namespace Orchard.CRM.Core.Services
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.Security;
    using System.Collections.Generic;

    public interface IBasicDataService : IDependency
    {
        IEnumerable<IUser> GetCustomers(string searchPhrase, int pageId, int pageSize, UsersOrderViewModel orderBy, bool decsending);
        int GetCustomersCount(string searchPhrase);
        IUser GetOperatorOrCustomerUser(string email);
        IUser GetOperatorOrCustomerUser(int id);
        IEnumerable<ContentItem> GetTeams();
        IEnumerable<IUser> GetOperators();
        IEnumerable<ContentItem> GetBusinessUnits();
        IEnumerable<BusinessUnitMemberPartRecord> GetBusinessUnitMembers();
        IEnumerable<TeamMemberPartRecord> GetTeamMembers();
        IEnumerable<StatusRecord> GetStatusRecords();
        IEnumerable<ServicePart> GetServices();
        IEnumerable<TicketTypeRecord> GetTicketTypes();
        IEnumerable<PriorityRecord> GetPriorities();
        void ClearCache();
    }
}
