using Orchard.SuiteCRM.Connector.Models;
using Orchard.SuiteCRM.Connector.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.CRM.Core.Models;
using Orchard.Logging;
using Orchard.CRM.Core.Services;
using Orchard.SuiteCRM.Connector.Database;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Project.Services;
using Orchard.Data;
using System.IO;
using Orchard.CRM.Project.Models;
using Orchard.UI.Navigation;
using Orchard.Security;
using System.Globalization;
using Orchard.Users.Models;
using Orchard.Roles.Models;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMSyncUsersService : ISuiteCRMSyncUserService
    {
        private readonly IOrchardServices services;
        private readonly IMembershipService membershipService;
        private readonly IRepository<RolesPermissionsRecord> rolesPermissionsRepository;
        private readonly IRepository<RoleRecord> roleRepository;
        private readonly IRepository<UserRolesPartRecord> userRolesPartRecordRepository;

        public const string UsersBean = "Users";

        public SuiteCRMSyncUsersService(
            IRepository<UserRolesPartRecord> userRolesPartRecordRepository,
            IMembershipService membershipService,
            IRepository<RolesPermissionsRecord> rolesPermissionsRepository,
            IRepository<RoleRecord> roleRepository,
            IOrchardServices services)
        {
            this.userRolesPartRecordRepository = userRolesPartRecordRepository;
            this.membershipService = membershipService;
            this.rolesPermissionsRepository = rolesPermissionsRepository;
            this.roleRepository = roleRepository;
            this.services = services;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public int GetSuiteCRMUsersCount()
        {
            using (var connection = Helper.GetConnection(this.services, this.Logger))
            using (SuiteCRMUserUnitOfWork userRepository = new SuiteCRMUserUnitOfWork(connection))
            {
                return userRepository.Count();
            }
        }

        public IEnumerable<SuiteCRMUserViewModel> GetUsers(int pageNumber, int pageSize)
        {
            List<SuiteCRMUserViewModel> returnValue = new List<SuiteCRMUserViewModel>();

            using (var connection = Helper.GetConnection(this.services, this.Logger))
            using (SuiteCRMUserUnitOfWork userRepository = new SuiteCRMUserUnitOfWork(connection))
            using (SuiteCRMEmailAddressUnitOfWork emailRepository = new SuiteCRMEmailAddressUnitOfWork(connection))
            using (SuiteCRMEmailAddressBeanUnitOfWork emailBeanRepository = new SuiteCRMEmailAddressBeanUnitOfWork(connection))
            {
                var suiteUsers = userRepository.GetUsers(pageNumber, pageSize);
                var ids = suiteUsers.Select(c => c.id).ToArray();
                var suiteUserEmailBeans = emailBeanRepository.GetEmailAddressesBean(UsersBean, ids).ToArray();
                var suiteUserEmails = emailRepository.GetEmailAddresses(suiteUserEmailBeans.Select(c => c.email_address_id).ToArray()).ToList();

                string[] usernames = suiteUsers.Select(c=>c.user_name).ToArray();
                var orchardUsers = this.services.ContentManager.HqlQuery<UserPart>().Where(c => c.ContentPartRecord<UserPartRecord>(), c => c.In("UserName", usernames)).List();
                
                foreach (var suiteUser in suiteUsers)
                {
                    var suiteCRMEmailBean = suiteUserEmailBeans.FirstOrDefault(c => c.bean_id == suiteUser.id);
                    if (suiteCRMEmailBean == null)
                    {
                        continue;
                    }

                    var suiteCRMUserEmail = suiteUserEmails.FirstOrDefault(c => c.id == suiteCRMEmailBean.email_address_id);
                    if (suiteCRMUserEmail == null)
                    {
                        continue;
                    }

                    SuiteCRMUserViewModel item = new SuiteCRMUserViewModel();
                    item.SuiteCRMUsername = suiteUser.user_name;
                    item.SuiteCRMUserId = suiteUser.id;
                    item.SuiteCRMEmail = suiteCRMUserEmail.email_address;

                    var user = orchardUsers.FirstOrDefault(c => 
                        c.UserName.ToLower() == suiteUser.user_name.ToLower() &&
                        c.Email != null &&
                        c.Email.ToLower() == suiteCRMUserEmail.email_address.ToLower());
                    if (user != null)
                    {
                        item.IsSync = true;
                        item.OrchardUsername = user.UserName;
                        item.OrchardEmail = user.Email;
                        item.OrchardUserId = user.Id;
                    }

                    returnValue.Add(item);
                }
            }

            return returnValue;
        }

        public IEnumerable<SuiteCRMUserViewModel> CopySuiteCRMUsersToOrchard(CopySuiteCRMUsersToOrchardViewModel model)
        {
            List<SuiteCRMUserViewModel> returnValue = new List<SuiteCRMUserViewModel>();

            using (var connection = Helper.GetConnection(this.services, this.Logger))
            using (SuiteCRMUserUnitOfWork userRepository = new SuiteCRMUserUnitOfWork(connection))
            using (SuiteCRMEmailAddressUnitOfWork emailRepository = new SuiteCRMEmailAddressUnitOfWork(connection))
            using (SuiteCRMEmailAddressBeanUnitOfWork emailBeanRepository = new SuiteCRMEmailAddressBeanUnitOfWork(connection))
            {
                var ids = model.Users.Where(c => !string.IsNullOrEmpty(c.SuiteCRMUserId)).Select(c => c.SuiteCRMUserId).ToArray();
                var suiteUsers = userRepository.GetUsers(ids);
                var suiteUserEmailBeans = emailBeanRepository.GetEmailAddressesBean(UsersBean, ids).ToArray();
                var suiteUserEmails = emailRepository.GetEmailAddresses(suiteUserEmailBeans.Select(c => c.email_address_id).ToArray()).ToList();
                var orchardUsers = this.services
                    .ContentManager
                    .GetMany<IUser>(model.Users.Where(c => c.OrchardUserId.HasValue).Select(c => c.OrchardUserId.Value), VersionOptions.Published, new QueryHints());

                var operatorRole = this.GetOperatorRole();
                foreach (var item in model.Users.Where(c => !string.IsNullOrEmpty(c.SuiteCRMUserId)))
                {
                    var suiteCRMUser = suiteUsers.FirstOrDefault(c => c.id == item.SuiteCRMUserId);

                    if (suiteCRMUser == null)
                    {
                        continue;
                    }

                    var suiteCRMEmailBean = suiteUserEmailBeans.FirstOrDefault(c => c.bean_id == item.SuiteCRMUserId);
                    if (suiteCRMEmailBean == null)
                    {
                        continue;
                    }

                    var suiteCRMUserEmail = suiteUserEmails.FirstOrDefault(c => c.id == suiteCRMEmailBean.email_address_id);
                    if (suiteCRMUserEmail == null)
                    {
                        continue;
                    }

                    IUser user = orchardUsers.FirstOrDefault(c =>
                        c.UserName.ToLower() == suiteCRMUser.user_name.ToLower() ||
                        c.Email.ToLower() == suiteCRMUserEmail.email_address.ToLower());

                    if (user != null)
                    {
                        continue;
                    }

                    var newUser = this.membershipService.CreateUser(new CreateUserParams(
                                                  suiteCRMUser.user_name,
                                                  model.DefaultPassword,
                                                  suiteCRMUserEmail.email_address,
                                                  null, null, true));

                    SuiteCRMUserPart suiteCRMUserPart = newUser.As<SuiteCRMUserPart>();
                    suiteCRMUserPart.ExternalId = suiteCRMUser.id;
                    suiteCRMUserPart.LastSyncTime = DateTime.UtcNow;

                    // Full name
                    var userPart = newUser.ContentItem.Parts.FirstOrDefault(d => d.PartDefinition.Name.ToLower(CultureInfo.InvariantCulture) == "user");
                    userPart.Store("FullName", string.Format("{0} {1}", suiteCRMUser.first_name, suiteCRMUser.last_name));

                    // role
                    if (operatorRole != null)
                    {
                        UserRolesPartRecord newUserRole = new UserRolesPartRecord { UserId = newUser.Id, Role = new RoleRecord { Id = operatorRole.Id } };
                        this.userRolesPartRecordRepository.Create(newUserRole);
                    }

                    this.services.ContentManager.Publish(newUser.ContentItem);
                    this.userRolesPartRecordRepository.Flush();

                    returnValue.Add(new SuiteCRMUserViewModel
                    {
                        IsSync = true,
                        OrchardEmail = newUser.Email,
                        OrchardUserId = newUser.Id,
                        SuiteCRMUserId = suiteCRMUser.id,
                        SuiteCRMEmail = suiteCRMUserEmail.email_address,
                        SuiteCRMUsername = suiteCRMUser.user_name,
                        OrchardUsername = newUser.UserName
                    });
                }
            }

            return returnValue;
        }

        private RoleRecord GetOperatorRole()
        {
            var roleIds = this.rolesPermissionsRepository.Table.Where(c =>
                (c.Permission.Name == Orchard.CRM.Core.Permissions.OperatorPermission.Name) &&
                c.Permission.FeatureName == "Orchard.CRM.Core").Select(c => c.Role.Id).ToArray();

            var roles = this.roleRepository.Table.Where(c => roleIds.Contains(c.Id)).ToList();

            var operatorRole = roles.FirstOrDefault(c => c.Name == "Operator");

            return operatorRole != null ? operatorRole : roles.FirstOrDefault();
        }
    }
}