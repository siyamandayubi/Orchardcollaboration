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

namespace Orchard.CRM.Core.Controllers
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.Data;
    using Orchard.DisplayManagement.Implementation;
    using Orchard.Localization;
    using Orchard.Roles.Models;
    using Orchard.Users.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    
    public class UsersController : Controller, IUpdateModel
    {
        private IRepository<UserRolesPartRecord> userRoleRepository;
        private IRepository<UserPartRecord> userRepository;
        private IDisplayManager displayManager;

        private IOrchardServices services;

        public const int PageSize = 128;

        public UsersController(
            IOrchardServices services,
            IDisplayManager displayManager,
            IRepository<UserRolesPartRecord> userRoleRepository,
            IRepository<UserPartRecord> userRepository)
        {
            this.displayManager = displayManager;
            this.services = services;
            this.userRepository = userRepository;
            this.userRoleRepository = userRoleRepository;
        }

        public ActionResult Index(int? roleId)
        {
            IEnumerable<int> userIds = null;
            if (roleId.HasValue)
            {
                userIds = this.userRoleRepository.Table.Where(c => c.Role.Id == roleId).Select(c => c.UserId).ToList();
            }
            else
            {
                userIds = this.userRepository.Table.Select(c => c.Id).ToList();
            }

            VersionOptions versionOptions = new VersionOptions();
            var users = this.services.ContentManager.GetMany<UserPart>(userIds, VersionOptions.Published, new QueryHints());

            bool isAjaxRequest = Request.IsAjaxRequest();
            if (isAjaxRequest)
            {
                AjaxMessageViewModel ajaxMessageModel = new AjaxMessageViewModel { IsDone = true };
                ajaxMessageModel.Html = this.RenderPartialViewToString("Users", users);

                return this.Json(ajaxMessageModel, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return this.View((object)users);
            }
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