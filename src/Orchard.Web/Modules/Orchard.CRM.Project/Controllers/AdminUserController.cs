using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;
using Orchard.Mvc.Extensions;
using System;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;
using Orchard.Users;
using Orchard.CRM.Core.Services;
using System.Globalization;
using Orchard.CRM.Core.Providers.ActivityStream;
using Orchard.Themes;
using Orchard.Environment.Configuration;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Roles.ViewModels;

namespace Orchard.CRM.Project.Controllers
{
    [ValidateInput(false)]
    [Themed(true)]
    public class AdminUserController : Controller, IUpdateModel
    {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly ISiteService _siteService;
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly IActivityStreamService activityStreamService;
        private readonly IRepository<RolesPermissionsRecord> rolesPermissionsRepository;

        public AdminUserController(
            IRepository<RolesPermissionsRecord> rolesPermissionsRepository,
            IActivityStreamService activityStreamService,
            ICRMContentOwnershipService crmContentOwnershipService,
            IOrchardServices services,
            IMembershipService membershipService,
            IUserService userService,
            IShapeFactory shapeFactory,
            IUserEventHandler userEventHandlers,
            ISiteService siteService)
        {
            this.rolesPermissionsRepository = rolesPermissionsRepository;
            this.activityStreamService = activityStreamService;
            this.crmContentOwnershipService = crmContentOwnershipService;
            Services = services;
            _membershipService = membershipService;
            _userService = userService;
            _userEventHandlers = userEventHandlers;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(UserIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to list users")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new UserIndexOptions();

            var users = Services.ContentManager
                .Query<UserPart, UserPartRecord>();

            switch (options.Filter)
            {
                case UsersFilter.Approved:
                    users = users.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
                case UsersFilter.Pending:
                    users = users.Where(u => u.RegistrationStatus == UserStatus.Pending);
                    break;
                case UsersFilter.EmailPending:
                    users = users.Where(u => u.EmailStatus == UserStatus.Pending);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                users = users.Where(u => u.UserName.Contains(options.Search) || u.Email.Contains(options.Search));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(users.Count());

            switch (options.Order)
            {
                case UsersOrder.Name:
                    users = users.OrderBy(u => u.UserName);
                    break;
                case UsersOrder.Email:
                    users = users.OrderBy(u => u.Email);
                    break;
            }

            var results = users
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var shellSettings = this.Services.WorkContext.Resolve<ShellSettings>();
            var siteSettings = _siteService.GetSiteSettings();
            if (!string.IsNullOrEmpty(shellSettings.RequestUrlPrefix))
            {
                results = results.Where(c => !string.Equals(c.UserName, siteSettings.SuperUser, StringComparison.Ordinal)).ToList();
            }

            var model = new UsersIndexViewModel
            {
                Users = results
                    .Select(x => new UserEntry { User = x.Record })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var viewModel = new UsersIndexViewModel { Users = new List<UserEntry>(), Options = new UserIndexOptions() };
            UpdateModel(viewModel);

            var checkedEntries = viewModel.Users.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case UsersBulkAction.None:
                    break;
                case UsersBulkAction.Approve:
                    foreach (var entry in checkedEntries)
                    {
                        Approve(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.Disable:
                    foreach (var entry in checkedEntries)
                    {
                        Moderate(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.ChallengeEmail:
                    foreach (var entry in checkedEntries)
                    {
                        SendChallengeEmail(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.Delete:
                    foreach (var entry in checkedEntries)
                    {
                        Delete(entry.User.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string defaultRole)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.New<IUser>("User");
            var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Create", Model: new UserCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            var model = Services.ContentManager.BuildEditor(user);
            model.Content.Items.Insert(0, editor);
            FilterRolesToItemsWithCRMCorePermissions(model, defaultRole);
            return View(model);
        }

        private void FilterRolesToItemsWithCRMCorePermissions(dynamic model, string defaultRole)
        {
            var allowedRoles = this.rolesPermissionsRepository.Table.Where(c =>
            (c.Permission.Name == Orchard.CRM.Core.Permissions.OperatorPermission.Name ||
            c.Permission.Name == Orchard.CRM.Core.Permissions.CustomerPermission.Name ||
            c.Permission.Name == Orchard.CRM.Core.Permissions.AdvancedOperatorPermission.Name) &&
            c.Permission.FeatureName == "Orchard.CRM.Core").Select(c => c.Role.Id).ToArray();

            if (model.Content == null || model.Content.Items == null)
            {
                return;
            }

            foreach (var item in model.Content.Items)
            {
                if (item.Prefix == "UserRoles")
                {
                    UserRolesViewModel viewModel = item.Model;
                    viewModel
                        .Roles
                        .Where(role => !allowedRoles.Any(r => r == role.RoleId))
                        .ToList()
                        .ForEach(c => viewModel.Roles.Remove(c));

                    viewModel.Roles.Where(c => c.Name == defaultRole).ToList().ForEach(c => c.Granted = true);
                }
            }
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(UserCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.UserName))
            {
                if (!_userService.VerifyUserUnicity(createModel.UserName, createModel.Email))
                {
                    AddModelError("NotUniqueUserName", T("User with that username and/or email already exists."));
                }
            }

            if (!Regex.IsMatch(createModel.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase))
            {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                ModelState.AddModelError("Email", T("You must specify a valid email address."));
            }

            if (createModel.Password != createModel.ConfirmPassword)
            {
                AddModelError("ConfirmPassword", T("Password confirmation must match"));
            }

            var user = Services.ContentManager.New<IUser>("User");
            if (ModelState.IsValid)
            {
                user = _membershipService.CreateUser(new CreateUserParams(
                                                  createModel.UserName,
                                                  createModel.Password,
                                                  createModel.Email,
                                                  null, null, true));
            }

            var model = Services.ContentManager.UpdateEditor(user, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Items.Insert(0, editor);

                return View(model);
            }

            RouteValueDictionary activityStreamRouteValues = new RouteValueDictionary();
            activityStreamRouteValues.Add("action", "Display");
            activityStreamRouteValues.Add("controller", "User");
            activityStreamRouteValues.Add("area", "orchard.crm.Project");
            activityStreamRouteValues.Add("userId", user.Id);

            string changeDescription = string.Format(CultureInfo.CurrentUICulture, "Creates new user '{0}'", CRMHelper.GetFullNameOfUser(user));
            this.activityStreamService.WriteChangesToStreamActivity(Services.WorkContext.CurrentUser.Id, user.Id, user.ContentItem.VersionRecord.Id, new ActivityStreamChangeItem[] { }, changeDescription, activityStreamRouteValues);

            string actionName = this.crmContentOwnershipService.IsOperator(user.Id) ? "Operators" : "Customers";
            return RedirectToAction(actionName, new { Controller = "User", area = "Orchard.CRM.Project" });
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserPart>(id);
            var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Edit", Model: new UserEditViewModel { User = user }, Prefix: null);
            editor.Metadata.Position = "2";
            var model = Services.ContentManager.BuildEditor(user);
            model.Content.Items.Insert(0, editor);
            FilterRolesToItemsWithCRMCorePermissions(model, string.Empty);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserPart>(id, VersionOptions.DraftRequired);
            string previousName = user.UserName;

            var shellSettings = this.Services.WorkContext.Resolve<ShellSettings>();
            var siteSettings = _siteService.GetSiteSettings();
            if (!string.IsNullOrEmpty(shellSettings.RequestUrlPrefix) && string.Equals(user.UserName, siteSettings.SuperUser, StringComparison.Ordinal))
            {
                throw new OrchardSecurityException(T("You don't have permission to moderate the user"));
            }

            var model = Services.ContentManager.UpdateEditor(user, this);

            var editModel = new UserEditViewModel { User = user };
            if (TryUpdateModel(editModel))
            {
                if (!_userService.VerifyUserUnicity(id, editModel.UserName, editModel.Email))
                {
                    AddModelError("NotUniqueUserName", T("User with that username and/or email already exists."));
                }
                else if (!Regex.IsMatch(editModel.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase))
                {
                    // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                    ModelState.AddModelError("Email", T("You must specify a valid email address."));
                }
                else
                {
                    // also update the Super user if this is the renamed account
                    if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, previousName, StringComparison.Ordinal))
                    {
                        _siteService.GetSiteSettings().As<SiteSettingsPart>().SuperUser = editModel.UserName;
                    }

                    user.NormalizedUserName = editModel.UserName.ToLowerInvariant();
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/User.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Items.Insert(0, editor);

                return View(model);
            }

            Services.ContentManager.Publish(user.ContentItem);

            RouteValueDictionary activityStreamRouteValues = new RouteValueDictionary();
            activityStreamRouteValues.Add("action", "Display");
            activityStreamRouteValues.Add("controller", "User");
            activityStreamRouteValues.Add("area", "orchard.crm.Project");
            activityStreamRouteValues.Add("userId", user.Id);

            string changeDescription = string.Format(CultureInfo.CurrentUICulture, "Update user '{0}'", CRMHelper.GetFullNameOfUser(user));
            this.activityStreamService.WriteChangesToStreamActivity(Services.WorkContext.CurrentUser.Id, user.Id, user.ContentItem.VersionRecord.Id, new ActivityStreamChangeItem[] { }, changeDescription, activityStreamRouteValues);

            return RedirectToAction("Display", new { Controller = "User", area = "Orchard.CRM.Project", userId = user.Id });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                var shellSettings = this.Services.WorkContext.Resolve<ShellSettings>();
                var siteSettings = _siteService.GetSiteSettings();
                if (!string.IsNullOrEmpty(shellSettings.RequestUrlPrefix) && string.Equals(user.UserName, siteSettings.SuperUser, StringComparison.Ordinal))
                {
                    throw new OrchardSecurityException(T("You don't have permission to moderate the user"));
                }

                if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, user.UserName, StringComparison.Ordinal))
                {
                    Services.Notifier.Error(T("The Super user can't be removed. Please disable this account or specify another Super user account."));
                }
                else if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.Ordinal))
                {
                    Services.Notifier.Error(T("You can't remove your own account. Please log in with another account."));
                }
                else
                {
                    Services.ContentManager.Remove(user.ContentItem);
                    string changeDescription = string.Format(CultureInfo.CurrentUICulture, "Delete the user '{0}'", CRMHelper.GetFullNameOfUser(user));
                    this.activityStreamService.WriteChangesToStreamActivity(Services.WorkContext.CurrentUser.Id, user.Id, user.ContentItem.VersionRecord.Id, new ActivityStreamChangeItem[] { }, changeDescription, null);
                }
            }

            string actionName = this.crmContentOwnershipService.IsOperator(user.Id) ? "Operators" : "Customers";
            return RedirectToAction(actionName, new { Controller = "User", area = "Orchard.CRM.Project" });
        }

        [HttpPost]
        public ActionResult SendChallengeEmail(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                var siteUrl = Services.WorkContext.CurrentSite.BaseUrl;
                if (String.IsNullOrWhiteSpace(siteUrl))
                {
                    siteUrl = HttpContext.Request.ToRootUrlString();
                }

                _userService.SendChallengeEmail(user.As<UserPart>(), nonce => Url.MakeAbsolute(Url.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));
                Services.Notifier.Information(T("Challenge email sent to {0}", user.UserName));
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Approve(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                user.As<UserPart>().RegistrationStatus = UserStatus.Approved;
                Services.Notifier.Information(T("User {0} approved", user.UserName));
                _userEventHandlers.Approved(user);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Moderate(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                var shellSettings = this.Services.WorkContext.Resolve<ShellSettings>();
                var siteSettings = _siteService.GetSiteSettings();
                if (!string.IsNullOrEmpty(shellSettings.RequestUrlPrefix) && string.Equals(user.UserName, siteSettings.SuperUser, StringComparison.Ordinal))
                {
                    throw new OrchardSecurityException(T("You don't have permission to moderate the user"));
                }

                if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.Ordinal))
                {
                    Services.Notifier.Error(T("You can't disable your own account. Please log in with another account"));
                }
                else
                {
                    user.As<UserPart>().RegistrationStatus = UserStatus.Pending;
                    Services.Notifier.Information(T("User {0} disabled", user.UserName));
                }
            }

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}
