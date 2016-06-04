/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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
        IEnumerable<ServiceRecord> GetServices();
        IEnumerable<TicketTypeRecord> GetTicketTypes();
        IEnumerable<PriorityRecord> GetPriorities();
        void ClearCache();
    }
}
