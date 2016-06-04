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
    using Orchard.Caching;
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.Data;
    using Orchard.Projections.Models;
    using Orchard.Roles.Models;
    using Orchard.Security;
    using Orchard.Users.Models;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class BasicDataService : IBasicDataService
    {
        private readonly static Object ourLock = new Object();

        private readonly IRepository<ServiceRecord> servicePartRecordRepository;
        private readonly IRepository<TicketTypeRecord> ticketTypeRecordRepository;
        private readonly IRepository<RolesPermissionsRecord> rolesPermissionsRepository;
        private readonly ICacheManager chacheManager;
        private readonly IContentManager contentManager;
        private readonly IRepository<StatusRecord> statusRecordRepository;
        private readonly IRepository<PriorityRecord> priorityRecordRepository;
        private readonly IRepository<BusinessUnitMemberPartRecord> businessUnitMemberRepository;
        private readonly IRepository<TeamMemberPartRecord> teamMemberRepository;
        private readonly IOrchardServices services;
        protected readonly IRepository<UserRolesPartRecord> userRolesRepository;
        protected readonly IRepository<UserPartRecord> userRepository;
        protected readonly IRepository<FieldIndexPartRecord> fieldRepository;


        private static bool RefreshServices = false;
        private static bool RefreshStatusRecords = false;
        private static bool RefreshPriorities = false;
        private static bool RefreshTicketTypes = false;
        private static bool RefreshBusinessUnitMembers = false;
        private static bool RefreshTeamMembers = false;
        private static bool RefreshBusinessUnits = false;
        private static bool RefreshTeams = false;
        private static bool RefreshOperatorUsers = false;
        private static bool RefreshCustomerAndOperatorUsers = false;
        private static int CustomersAndOperatorsCount = 0;

        public BasicDataService(
            IRepository<FieldIndexPartRecord> fieldRepository,
            IRepository<UserPartRecord> userRepository,
            ICacheManager chacheManager,
            IOrchardServices services,
            IContentManager contentManager,
            IRepository<PriorityRecord> priorityRecordRepository,
            IRepository<StatusRecord> statusRecordRepository,
            IRepository<BusinessUnitMemberPartRecord> businessUnitMemberRepository,
            IRepository<TicketTypeRecord> ticketTypeRecordRepository,
            IRepository<TeamMemberPartRecord> teamMemberRepository,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IRepository<RolesPermissionsRecord> rolesPermissionsRepository,
            IRepository<ServiceRecord> servicePartRecordRepository)
        {
            this.fieldRepository = fieldRepository;
            this.userRepository = userRepository;
            this.services = services;
            this.rolesPermissionsRepository = rolesPermissionsRepository;
            this.userRolesRepository = userRolesRepository;
            this.teamMemberRepository = teamMemberRepository;
            this.statusRecordRepository = statusRecordRepository;
            this.chacheManager = chacheManager;
            this.priorityRecordRepository = priorityRecordRepository;
            this.ticketTypeRecordRepository = ticketTypeRecordRepository;
            this.servicePartRecordRepository = servicePartRecordRepository;
            this.businessUnitMemberRepository = businessUnitMemberRepository;
            this.contentManager = contentManager;
        }

        public void ClearCache()
        {
            lock (ourLock)
            {
                RefreshServices = true;
                RefreshStatusRecords = true;
                RefreshPriorities = true;
                RefreshTicketTypes = true;
                RefreshBusinessUnitMembers = true;
                RefreshTeamMembers = true;
                RefreshBusinessUnits = true;
                RefreshTeams = true;
                RefreshOperatorUsers = true;
                RefreshCustomerAndOperatorUsers = true;
            }
        }

        public IEnumerable<BusinessUnitMemberPartRecord> GetBusinessUnitMembers()
        {
            return this.chacheManager.Get("BusinessUnitMembers", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshBusinessUnitMembers));
                var temp = this.contentManager.Query().ForType("BusinessUnitMember").ForVersion(VersionOptions.Published).List();

                var returnValue = temp.Select(c => c.As<BusinessUnitMemberPart>().Record);

                lock (ourLock)
                {
                    RefreshBusinessUnitMembers = false;
                }

                return returnValue;
            });
        }

        public IEnumerable<ContentItem> GetBusinessUnits()
        {
            return this.chacheManager.Get("BusinessUnits", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshBusinessUnits));
                var returnValue = this.contentManager.HqlQuery().ForType("BusinessUnit").List();

                lock (ourLock)
                {
                    RefreshBusinessUnits = false;
                }

                return returnValue;
            });
        }

        public int GetCustomersCount(string searchPhrase)
        {
            var operatorsAndCustomers = this.GetOperatorAndCustomers();

            // cache values
            if (false && operatorsAndCustomers != null)
            {
                var operators = this.GetOperators();
                var customers = operators.Count() > 0 ?
                    operatorsAndCustomers.Where(c => !operators.Any(d => d.Id == c.Id)).ToList() :
                    operatorsAndCustomers;

                if (!string.IsNullOrEmpty(searchPhrase))
                {
                    searchPhrase = searchPhrase.ToLower(CultureInfo.InvariantCulture);
                    customers = customers.Where(c =>
                        c.Email.ToLower().Contains(searchPhrase) ||
                        c.UserName.ToLower().Contains(searchPhrase) ||
                        CRMHelper.GetFullNameOfUser(c).ToLower().Contains(searchPhrase)).ToList();
                }

                return customers.Count;
            }
            else
            {
                var temp = this.rolesPermissionsRepository.Table.Where(c =>
                                   c.Permission.Name == Permissions.CustomerPermission.Name &&
                                   c.Permission.FeatureName == "Orchard.CRM.Core").ToList();

                var customerRoles = temp.ConvertAll(c => c.Role.Id).ToArray();

                searchPhrase = searchPhrase != null ? searchPhrase.ToLower(CultureInfo.InvariantCulture) : string.Empty;
                var customers = (from userRole in this.userRolesRepository.Table
                                 join
                                 user in this.userRepository.Table
                                 on userRole.UserId equals user.Id
                                 join f in fieldRepository.Table
                                 on user.Id equals f.ContentItemRecord.Id

                                 where customerRoles.Contains(userRole.Role.Id) &&
                                 (searchPhrase == null ||
                                 user.UserName.Contains(searchPhrase) ||
                                 user.Email.Contains(searchPhrase) ||
                                 f.StringFieldIndexRecords.Any(d => d.Value.Contains(searchPhrase) && d.PropertyName == "User.FullName")
                                 )
                                 select user);

                return customers.Count();
            }
        }
        public IEnumerable<IUser> GetCustomers(string searchPhrase, int pageId, int pageSize, UsersOrderViewModel orderBy, bool decsending)
        {
            if (pageId <= 0)
            {
                pageId = 1;
            }
            var operatorsAndCustomers = this.GetOperatorAndCustomers();

            // cache values
            if (false && operatorsAndCustomers != null)
            {
                var operators = this.GetOperators();
                var customers = operators.Count() > 0 ?
                    operatorsAndCustomers.Where(c => !operators.Any(d => d.Id == c.Id)).ToList() :
                    operatorsAndCustomers;

                // TODO: fix the issue of checking user tags
                if (!string.IsNullOrEmpty(searchPhrase))
                {
                    searchPhrase = searchPhrase.ToLower(CultureInfo.InvariantCulture);
                    customers = customers.Where(c =>
                        c.Email.ToLower().Contains(searchPhrase) ||
                        c.UserName.ToLower().Contains(searchPhrase) ||
                        CRMHelper.GetFullNameOfUser(c).ToLower().Contains(searchPhrase)).ToList();
                }

                switch (orderBy)
                {
                    case UsersOrderViewModel.Default:
                        customers = decsending ? customers.OrderBy(c => c.Id).ToList() : customers.OrderByDescending(c => c.Id).ToList();
                        break;
                    case UsersOrderViewModel.Email:
                        customers = decsending ? customers.OrderBy(c => c.Email).ToList() : customers.OrderByDescending(c => c.Email).ToList();
                        break;
                    case UsersOrderViewModel.Username:
                        customers = decsending ? customers.OrderBy(c => c.UserName).ToList() : customers.OrderByDescending(c => c.UserName).ToList();
                        break;
                }

                return customers.Skip((pageId - 1) * pageSize).Take(pageSize);
            }
            else
            {
                var temp = this.rolesPermissionsRepository.Table.Where(c =>
                    c.Permission.Name == Permissions.CustomerPermission.Name &&
                    c.Permission.FeatureName == "Orchard.CRM.Core").ToList();

                var customerRoles = temp.ConvertAll(c => c.Role.Id).ToArray();

                searchPhrase = searchPhrase != null ? searchPhrase.ToLower(CultureInfo.InvariantCulture) : string.Empty;
                var customers = (from userRole in this.userRolesRepository.Table
                                 join
                                 user in this.userRepository.Table
                                 on userRole.UserId equals user.Id
                                 join f in fieldRepository.Table
                                 on user.Id equals f.ContentItemRecord.Id

                                 where customerRoles.Contains(userRole.Role.Id) &&
                                 (searchPhrase == null ||
                                 user.UserName.Contains(searchPhrase) ||
                                 user.Email.Contains(searchPhrase) ||
                                 f.StringFieldIndexRecords.Any(d => d.Value.Contains(searchPhrase) && (d.PropertyName == "User.FullName" || d.PropertyName == "User.Tags"))
                                 )
                                 select user);

                switch (orderBy)
                {
                    case UsersOrderViewModel.Default:
                        customers = decsending ? customers.OrderBy(c => c.Id) : customers.OrderByDescending(c => c.Id);
                        break;
                    case UsersOrderViewModel.Email:
                        customers = decsending ? customers.OrderBy(c => c.Email) : customers.OrderByDescending(c => c.Email);
                        break;
                    case UsersOrderViewModel.Username:
                        customers = decsending ? customers.OrderBy(c => c.UserName) : customers.OrderByDescending(c => c.UserName);
                        break;
                }

                List<int> ids = customers.Skip((pageId - 1) * pageSize).Take(pageSize).Select(c => c.Id).ToList();

                return this.contentManager.GetMany<UserPart>(ids, VersionOptions.Published, new QueryHints()).Select(c => c.As<IUser>()).ToList();
            }
        }

        public IEnumerable<IUser> GetOperators()
        {
            return this.chacheManager.Get("Operators", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshOperatorUsers));

                var roles = this.rolesPermissionsRepository.Table.Where(c =>
                    (c.Permission.Name == Permissions.OperatorPermission.Name ||
                    c.Permission.Name == Permissions.AdvancedOperatorPermission.Name) &&
                    c.Permission.FeatureName == "Orchard.CRM.Core").Select(c => c.Role.Id).ToArray();

                var userRoles = this.userRolesRepository.Table.Where(c =>
                roles.Contains(c.Role.Id)).ToList();

                IEnumerable<int> userIds = userRoles.Select(c => c.UserId).Distinct();
                var users = this.contentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);

                lock (ourLock)
                {
                    RefreshOperatorUsers = false;
                }

                return users;
            });
        }

        public IUser GetOperatorOrCustomerUser(int id)
        {
            IUser user = null;
            if (CustomersAndOperatorsCount < 2000)
            {
                var customerAndOperators = this.GetOperatorAndCustomers();
                if (customerAndOperators != null)
                {
                    user = customerAndOperators.FirstOrDefault(c => c.Id == id);

                    // TODO: think about it, user can not be customer or operator, maybe the name of method must be changed
                    if (user == null)
                    {
                        user = this.contentManager.Get<IUser>(id);
                        return user;
                    }
                }
            }

            // TODO: think about it, user can not be customer or operator, maybe the name of method must be changed
            user = this.contentManager.Get<IUser>(id);
            return user;
        }

        public IUser GetOperatorOrCustomerUser(string email)
        {
            var user = this.contentManager.Query<UserPart, UserPartRecord>().Where(c => c.Email == email).List().FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            var roles = this.rolesPermissionsRepository.Table.Where(c =>
                   (c.Permission.Name == Permissions.OperatorPermission.Name ||
                    c.Permission.Name == Permissions.CustomerPermission.Name ||
                    c.Permission.Name == Permissions.AdvancedOperatorPermission.Name) &&
                    c.Permission.FeatureName == "Orchard.CRM.Core").Select(c => c.Role.Id).ToArray();

            var userRoles = this.userRolesRepository.Table.Where(c =>
              roles.Contains(c.Role.Id) && c.UserId == user.Id).ToList();

            if (userRoles.Count == 0)
            {
                return null;
            }

            return user.As<IUser>();
        }
        private List<IUser> GetOperatorAndCustomers()
        {
            return this.chacheManager.Get("Operators", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshCustomerAndOperatorUsers));

                var roles = this.rolesPermissionsRepository.Table.Where(c =>
                    (c.Permission.Name == Permissions.OperatorPermission.Name ||
                    c.Permission.Name == Permissions.CustomerPermission.Name ||
                    c.Permission.Name == Permissions.AdvancedOperatorPermission.Name) &&
                    c.Permission.FeatureName == "Orchard.CRM.Core").Select(c => c.Role.Id).ToArray();

                CustomersAndOperatorsCount = this.userRolesRepository.Table.Count(c =>
                roles.Contains(c.Role.Id));

                if (CustomersAndOperatorsCount > 2000)
                {
                    return null;
                }

                var userRoles = this.userRolesRepository.Table.Where(c =>
                roles.Contains(c.Role.Id)).ToList();

                IEnumerable<int> userIds = userRoles.Select(c => c.UserId).Distinct();
                var users = this.contentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);

                lock (ourLock)
                {
                    RefreshCustomerAndOperatorUsers = false;
                }

                return users.ToList();
            });
        }

        public IEnumerable<ContentItem> GetTeams()
        {
            return this.chacheManager.Get("Teams", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshTeams));
                var returnValue = this.contentManager.HqlQuery().ForType("Team").List();

                lock (ourLock)
                {
                    RefreshTeams = false;
                }

                return returnValue;
            });
        }

        public IEnumerable<TeamMemberPartRecord> GetTeamMembers()
        {
            return this.chacheManager.Get("TeamMembers", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshTeamMembers));

                var temp = this.contentManager.Query().ForType("TeamMember").ForVersion(VersionOptions.Published).List();

                var returnValue = temp.Select(c => c.As<TeamMemberPart>().Record);

                lock (ourLock)
                {
                    RefreshTeamMembers = false;
                }

                return returnValue;
            });
        }

        public IEnumerable<ServiceRecord> GetServices()
        {
            return this.chacheManager.Get("TicketServices", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshServices));
                var returnValue = this.servicePartRecordRepository.Table.Where(c => c.Deleted == false).ToList();

                lock (ourLock)
                {
                    RefreshServices = false;
                }

                return returnValue;
            });
        }

        public IEnumerable<PriorityRecord> GetPriorities()
        {
            return this.chacheManager.Get("TicketPriorities", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshPriorities));
                var returnValue = this.priorityRecordRepository.Table.ToList();

                lock (ourLock)
                {
                    RefreshPriorities = false;
                }

                return returnValue;
            });
        }

        public IEnumerable<StatusRecord> GetStatusRecords()
        {
            return this.chacheManager.Get("StatusRecords", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshStatusRecords));
                var returnValue = this.statusRecordRepository.Table.Where(c => c.Deleted == false).OrderBy(c => c.OrderId).ToList();

                lock (ourLock)
                {
                    RefreshStatusRecords = false;
                }

                return returnValue;
            });
        }

        public IEnumerable<TicketTypeRecord> GetTicketTypes()
        {
            return this.chacheManager.Get("TicketTypes", context =>
            {
                context.Monitor(new SimpleBooleanToken(() => !RefreshTicketTypes));
                var returnValue = this.ticketTypeRecordRepository.Table.Where(c => c.Deleted == false).ToList();

                lock (ourLock)
                {
                    RefreshTicketTypes = false;
                }

                return returnValue;
            });
        }
    }
}