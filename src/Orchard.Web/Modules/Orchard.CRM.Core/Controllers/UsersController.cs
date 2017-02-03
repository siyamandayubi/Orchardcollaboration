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