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

using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Roles.Models;
using Orchard.UI.Admin;
using Orchard.CRM.Core.Services;

namespace Orchard.CRM.Core.Controllers
{
    [ValidateInput(false), Admin]
    public class OrganizationController : Controller, IUpdateModel
    {
        private IRepository<BusinessUnitPartRecord> businessUnitRepository;
        private IRepository<TeamPartRecord> teamRepository;
        private readonly IRepository<RolesPermissionsRecord> rolesPermissionsRepository;
        private IRepository<BusinessUnitMemberPartRecord> businessUnitMemberRepository;
        private IRepository<TeamMemberPartRecord> teamMemberRepository;
        private IRepository<UserPartRecord> userPartRepository;
        protected IRepository<UserRolesPartRecord> userRolesRepository;
        private IOrchardServices services;
        private IBasicDataService basicDataService;

        public Localizer T { get; set; }

        public OrganizationController(
            IRepository<BusinessUnitPartRecord> businessUnitRepository,
            IRepository<RolesPermissionsRecord> rolesPermissionsRepository,
            IRepository<TeamPartRecord> teamRepository,
            IRepository<BusinessUnitMemberPartRecord> businessUnitMemberRepository,
            IRepository<TeamMemberPartRecord> teamMemberRepository,
            IRepository<UserPartRecord> userPartRepository,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IBasicDataService basicDataService,
            IOrchardServices services)
        {
            this.rolesPermissionsRepository = rolesPermissionsRepository;
            this.basicDataService = basicDataService;
            this.T = NullLocalizer.Instance;
            this.services = services;
            this.userPartRepository = userPartRepository;
            this.teamMemberRepository = teamMemberRepository;
            this.teamRepository = teamRepository;
            this.businessUnitRepository = businessUnitRepository;
            this.businessUnitMemberRepository = businessUnitMemberRepository;
            this.userRolesRepository = userRolesRepository;
        }

        public ActionResult BusinessUnits()
        {
            var businessUnits = this.services.ContentManager.Query().ForPart<BusinessUnitPart>().List();

            var model = businessUnits.Select(c => this.Convert(c.Record));

            return this.View(model);
        }

        public ActionResult BusinessUnitMembers(int businessUnitId)
        {
            var businessUnit = this.businessUnitRepository.Table.FirstOrDefault(c => c.Id == businessUnitId);

            BusinessUnitMembersViewModel model = new BusinessUnitMembersViewModel();
            model.BusinessUnit = this.Convert(businessUnit);

            var userIds = this.GetOperators();
            var users = this.services.ContentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);
            var businessUnitMembers = this.businessUnitMemberRepository.Table.Where(c => c.BusinessUnitPartRecord.Id == businessUnitId).ToList();

            foreach (var user in users)
            {
                var member = businessUnitMembers.FirstOrDefault(c => c.UserPartRecord.Id == user.Id);
                model.Users.Add(new CheckableViewModel<UserPart>
                {
                    IsChecked = member != null,
                    Item = user.As<UserPart>()
                });
            }

            return this.View(model);
        }

        private List<int> GetOperators()
        {
            var roles = this.rolesPermissionsRepository.Table.Where(c =>
                 (c.Permission.Name == Permissions.OperatorPermission.Name ||
                 c.Permission.Name == Permissions.AdvancedOperatorPermission.Name) &&
                 c.Permission.FeatureName == "Orchard.CRM.Core").Select(c => c.Role.Id).ToArray();

            var userIds = this.userRolesRepository.Table.Where(c =>
              roles.Contains(c.Role.Id)).Select(c => c.UserId).Distinct().ToList();
            return userIds;
        }

        [HttpPost]
        public ActionResult BusinessUnitMembersPost(int businessUnitId, List<CheckableViewModel<int>> users)
        {
            var contentManager = this.services.ContentManager;
            var businessUnitMembers = this.businessUnitMemberRepository.Table.Where(c => c.BusinessUnitPartRecord.Id == businessUnitId).ToList();

            // add selected members
            var selectedUsers = users.Where(c => c.IsChecked);
            var operators = this.GetOperators();
            foreach (var item in selectedUsers)
            {
                if (!operators.Any(c => c == item.Item))
                {
                    throw new Security.OrchardSecurityException(T("The selected user is not an operator"));
                }

                if (businessUnitMembers.Count(c => c.UserPartRecord.Id == item.Item) == 0)
                {
                    var newContent = contentManager.New("BusinessUnitMember");
                    contentManager.Create(newContent);
                    var part = newContent.As<BusinessUnitMemberPart>();
                    var record = part.Record;
                    record.BusinessUnitPartRecord = new BusinessUnitPartRecord { Id = businessUnitId };
                    record.UserPartRecord = new UserPartRecord { Id = item.Item };
                    contentManager.UpdateEditor(newContent, this);
                    contentManager.Publish(newContent);
                }
            }

            // delete none selected members
            foreach (var member in businessUnitMembers)
            {
                if (selectedUsers.Count(c => c.Item == member.UserPartRecord.Id) == 0)
                {
                    var deletedContent = contentManager.Get(member.ContentItemRecord.Id);

                    if (deletedContent != null)
                    {
                        contentManager.Remove(deletedContent);
                    }

                    this.businessUnitMemberRepository.Delete(member);
                    this.businessUnitMemberRepository.Flush();
                }
            }

            this.basicDataService.ClearCache();

            return this.RedirectToAction("BusinessUnitMembers", new { businessUnitId = businessUnitId });
        }

        public ActionResult TeamMembers(int teamId)
        {
            var team = this.teamRepository.Table.FirstOrDefault(c => c.Id == teamId);

            TeamMembersViewModel model = new TeamMembersViewModel();
            model.Team = this.Convert(team);

            var users = this.services.ContentManager.HqlQuery().ForPart<UserPart>().List();
            var teamMembers = this.teamMemberRepository.Table.Where(c => c.TeamPartRecord.Id == teamId).ToList();

            foreach (var user in users)
            {
                var member = teamMembers.FirstOrDefault(c => c.UserPartRecord.Id == user.Id);
                model.Users.Add(new CheckableViewModel<UserPart>
                {
                    IsChecked = member != null,
                    Item = user
                });
            }

            return this.View(model);
        }

        [HttpPost]
        public ActionResult TeamMembersPost(int teamId, List<CheckableViewModel<int>> users)
        {
            var teamMembers = this.teamMemberRepository.Table.Where(c => c.TeamPartRecord.Id == teamId).ToList();

            // add selected members
            var selectedUsers = users.Where(c => c.IsChecked);
            foreach (var item in selectedUsers)
            {
                if (teamMembers.Count(c => c.UserPartRecord.Id == item.Item) == 0)
                {
                    var newMember = new TeamMemberPartRecord
                    {
                        UserPartRecord = new UserPartRecord { Id = item.Item },
                        TeamPartRecord = new TeamPartRecord { Id = teamId }
                    };

                    this.teamMemberRepository.Create(newMember);
                }
            }

            // delete none selected members
            foreach (var member in teamMembers)
            {
                if (selectedUsers.Count(c => c.Item == member.UserPartRecord.Id) == 0)
                {
                    this.teamMemberRepository.Delete(member);
                }
            }

            this.teamMemberRepository.Flush();
            this.basicDataService.ClearCache();

            return this.RedirectToAction("TeamMembers", new { id = teamId });
        }

        public ActionResult Teams(int businessUnitId)
        {
            var teams = this.services.ContentManager.Query().ForPart<TeamPart>().List();

            teams = teams.Where(c => c.Record.BusinessUnitPartRecord.Id == businessUnitId).ToList();
            var model = teams.Select(c => this.Convert(c.Record));

            return this.View(model);
        }

        public ActionResult CreateBusinessUnit()
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            return this.View(new BusinessUnitViewModel());
        }

        public ActionResult CreateTeam()
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            return this.View(new TeamViewModel());
        }

        public ActionResult EditBusinessUnit(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var businessUnit = this.businessUnitRepository.Table.FirstOrDefault(c => c.Id == id);

            var model = this.Convert(businessUnit);

            return this.View(model);
        }

        public ActionResult EditTeam(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var team = this.teamRepository.Table.FirstOrDefault(c => c.Id == id);

            var model = this.Convert(team);

            return this.View(model);
        }

        [HttpPost]
        public ActionResult EditBusinessUnitPost(BusinessUnitViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("EditBusinessUnit", model);
            }

            var businessUnit = this.businessUnitRepository.Table.FirstOrDefault(c => c.Id == model.BusinessUnitId);

            if (businessUnit == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no businessUnit with the given Id").ToString());
                return this.View(model);
            }

            businessUnit.Name = model.Name;
            businessUnit.Description = model.Description;
            this.businessUnitRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("BusinessUnits");
        }

        [HttpPost]
        public ActionResult EditTeamPost(TeamViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var team = this.teamRepository.Table.FirstOrDefault(c => c.Id == model.TeamId);

            if (team == null)
            {
                this.ModelState.AddModelError("Id", this.T("There is no team with the given Id").ToString());
                return this.View(model);
            }

            team.Name = model.Name;
            team.Description = model.Description;
            this.teamRepository.Flush();
            this.basicDataService.ClearCache();

            return RedirectToAction("Teams");
        }

        public ActionResult RemoveBusinessUnit(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var contentItem = this.services.ContentManager.Get(id);
            var businessUnit = contentItem.As<BusinessUnitPart>();

            if (businessUnit == null)
            {
                object model = this.T("There is no businessUnit with the given Id").ToString();
                return this.View("Error", model);
            }

            this.services.ContentManager.Remove(contentItem);
            this.basicDataService.ClearCache();

            return RedirectToAction("BusinessUnits");
        }

        public ActionResult RemoveTeam(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            var contentItem = this.services.ContentManager.Get(id);
            var team = contentItem.As<TeamPart>();

            if (team == null)
            {
                object model = this.T("There is no team with the given Id").ToString();
                return this.View("Error", model);
            }

            this.services.ContentManager.Remove(contentItem);
            this.basicDataService.ClearCache();

            return RedirectToAction("Teams");
        }

        [HttpPost]
        public ActionResult CreateBusinessUnitPost(BusinessUnitViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("CreateBusinessUnit", model);
            }

            var businessUnit = this.services.ContentManager.New("BusinessUnit");
            this.services.ContentManager.Create(businessUnit);
            BusinessUnitPart part = businessUnit.As<BusinessUnitPart>();
            BusinessUnitPartRecord businessUnitRecord = part.Record;

            businessUnitRecord.Name = model.Name;
            businessUnitRecord.Description = model.Description;
            this.services.ContentManager.Publish(businessUnit);
            this.basicDataService.ClearCache();

            return RedirectToAction("BusinessUnits");
        }

        [HttpPost]
        public ActionResult CreateTeamPost(TeamViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.BasicDataPermission))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("~/Views/BasicData/CreateTeam.cshtml", model);
            }

            var team = this.services.ContentManager.New("Team");
            this.services.ContentManager.Create(team);
            var part = team.As<TeamPart>();
            var record = part.Record;

            record.Name = model.Name;
            record.Description = model.Description;
            record.BusinessUnitPartRecord = new BusinessUnitPartRecord { Id = model.TeamId };

            this.services.ContentManager.Publish(team);
            this.basicDataService.ClearCache();
            return RedirectToAction("Teams");
        }

        private BusinessUnitViewModel Convert(BusinessUnitPartRecord record)
        {
            return new BusinessUnitViewModel
            {
                BusinessUnitId = record.Id,
                Name = record.Name,
                Description = record.Description
            };
        }

        private TeamViewModel Convert(TeamPartRecord record)
        {
            return new TeamViewModel
            {
                TeamId = record.Id,
                BusinessUnitId = record.BusinessUnitPartRecord.Id,
                Name = record.Name,
                Description = record.Description
            };
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

    }
}