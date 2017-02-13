using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.Users.Events;
using Orchard.Users.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.CRM.Core;
using Orchard.CRM.Core.ViewModels;
using System.Globalization;
using Orchard.CRM.Core.Models;
using System.Collections.ObjectModel;
using Orchard.CRM.Core.Controllers;
using Orchard.Indexing;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Core.Providers.PermissionProviders;

namespace Orchard.CRM.Project.Controllers
{
    [Themed(true)]
    public class UserController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IActivityStreamService activityStreamService;
        private readonly IOrchardServices services;
        private readonly IUserService _userService;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
        private readonly ISiteService _siteService;
        private readonly IBasicDataService basicDataService;
        private readonly IExtendedProjectService projectService;
        private readonly ICRMContentOwnershipService contentOwnershipService;
        private readonly IContentOwnershipHelper contentOwnershipHelper;
        private readonly IHelperService helperService;
        private readonly IIndexProvider indexProvider;
        protected readonly IMasterDetailPermissionManager masterDetailPermissionManager;

        public UserController(
            IMasterDetailPermissionManager masterDetailPermissionManager,
            IIndexProvider indexProvider,
            IContentOwnershipHelper contentOwnershipHelper,
            IExtendedProjectService projectService,
            IHelperService helperService,
            IOrchardServices services,
            IActivityStreamService activityStreamService,
            ICRMContentOwnershipService contentOwnershipService,
            IBasicDataService basicDataService,
            IMembershipService membershipService,
            IUserService userService,
            IShapeFactory shapeFactory,
            IEnumerable<IUserEventHandler> userEventHandlers,
            ISiteService siteService)
        {
            this.masterDetailPermissionManager = masterDetailPermissionManager;
            this.indexProvider = indexProvider;
            this.contentOwnershipHelper = contentOwnershipHelper;
            this.projectService = projectService;
            this.activityStreamService = activityStreamService;
            this.contentOwnershipService = contentOwnershipService;
            this.basicDataService = basicDataService;
            this.services = services;
            this.helperService = helperService;
            _membershipService = membershipService;
            _userService = userService;
            _userEventHandlers = userEventHandlers;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Operators(UserIndexOptionsViewModel options)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return new HttpUnauthorizedResult();
            }

            options = options ?? new UserIndexOptionsViewModel();
            var operators = this.basicDataService.GetOperators();

            string searchPhrase = string.IsNullOrEmpty(options.Search)? string.Empty: options.Search.ToLower();
            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                operators = operators.Where(u =>
                    u.UserName.ToLower().Contains(searchPhrase) ||
                    u.Email.ToLower().Contains(searchPhrase) ||
                    (CRMHelper.GetFullNameOfUser(u) != null && CRMHelper.GetFullNameOfUser(u).ToLower().Contains(searchPhrase)))
                    .ToList();
            }

            switch (options.Order)
            {
                case UsersOrderViewModel.Username:
                    operators = operators.OrderBy(u => u.UserName);
                    break;
                case UsersOrderViewModel.Email:
                    operators = operators.OrderBy(u => u.Email);
                    break;
            }

            var pager = this.helperService.ReterivePagerFromQueryString();
            var pagerShape = Shape.Pager(pager).PagerId("page");
            pagerShape.TotalItemCount(operators.Count());

            UserListViewModel model = new UserListViewModel
            {
                Pager = pagerShape,
                Search = searchPhrase,
                OrderBy = options.Order,
                OrderByDescending = options.OrderByDescending
            };

            operators = operators.Skip(Math.Min(0, pager.Page - 1) * pager.PageSize).Take(pager.PageSize);

            model.Users.AddRange(operators.Select(c => ProjectHelper.Convert<UserViewModel>(c)));

            var userProjects = this.projectService.GetUsersProjects(operators.Select(c => c.Id));

            foreach (var user in model.Users)
            {
                if (userProjects.ContainsKey(user.UserId))
                {
                    user.UserProjects.AddRange(userProjects[user.UserId].Select(c => new SelectListItem
                    {
                        Value = c.Record.Id.ToString(CultureInfo.InvariantCulture),
                        Text = c.Record.Title
                    }));
                }
            }

            return View(model);
        }

        public ActionResult Customers(UserIndexOptionsViewModel options)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return new HttpUnauthorizedResult();
            }

            var pager = this.helperService.ReterivePagerFromQueryString();

            options = options ?? new UserIndexOptionsViewModel();

            int count = this.basicDataService.GetCustomersCount(options.Search);
            var customers = this.basicDataService.GetCustomers(options.Search, pager.Page, pager.PageSize, options.Order, options.OrderByDescending);

            var pagerShape = Shape.Pager(pager).PagerId("page");
            pagerShape.TotalItemCount(count);

            UserListViewModel model = new UserListViewModel
            {
                Pager = pagerShape,
                Search = options.Search,
                OrderBy = options.Order,
                OrderByDescending = options.OrderByDescending
            };

            model.Users.AddRange(customers.Select(c => ProjectHelper.Convert<UserViewModel>(c)));

            var userProjects = this.projectService.GetUsersProjects(customers.Select(c => c.Id));

            foreach (var user in model.Users)
            {
                if (userProjects.ContainsKey(user.UserId))
                {
                    user.UserProjects.AddRange(userProjects[user.UserId].Select(c => new SelectListItem
                    {
                        Value = c.Record.Id.ToString(CultureInfo.InvariantCulture),
                        Text = c.Record.Title
                    }));
                }
            }

            return View(model);
        }

        public ActionResult Display(int userId)
        {
            // check user is operator or customer
            if (!this.contentOwnershipService.IsCurrentUserOperator() &&
                !this.contentOwnershipService.IsCurrentUserCustomer())
            {
                return HttpNotFound();
            }

            var user = this.services.ContentManager.Get<IUser>(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            UserDetailViewModel model = ProjectHelper.Convert<UserDetailViewModel>(user);

            // retrieving paging parameters
            var queryString = this.services.WorkContext.HttpContext.Request.QueryString;
            var pageKey = "page";
            var page = 0;
            int pageSize = 10;

            // don't try to page if not necessary
            if (queryString.AllKeys.Contains(pageKey))
            {
                int.TryParse(queryString[pageKey], out page);
            }

            var userProjects = this.projectService.GetUsersProjects(new[] { userId });

            if (userProjects.ContainsKey(userId))
            {
                model.UserProjects.AddRange(userProjects[userId].Select(c => new SelectListItem
                {
                    Value = c.Record.Id.ToString(CultureInfo.InvariantCulture),
                    Text = c.Record.Title
                }));
            }

            var count = this.activityStreamService.ActivityStreamOfGivenUserVisibleByCurrentUserCount(userId);
            var items = this.activityStreamService.ActivityStreamOfGivenUserVisibleByCurrentUser(userId, page > 0 ? page - 1 : 0, pageSize).ToList();

            model.ActivityStream = this.activityStreamService.CreateModel(items, count, page, pageSize);

            return View(model);
        }

        public ActionResult Invite(int userId)
        {
            if (!this.services.Authorizer.Authorize(Orchard.Users.Permissions.ManageUsers, T("Not authorized to list users")) ||
                  !this.services.Authorizer.Authorize(Orchard.CRM.Core.Permissions.AdvancedOperatorPermission, T("Not authorized to list users")))
            {
                return new HttpUnauthorizedResult();
            }

            var user = this.services.ContentManager.Get<IUser>(userId);

            if (user == null)
            {
                return new HttpNotFoundResult(T("There is no user with the given Id").Text);
            }

            if (!this.contentOwnershipService.IsCustomer(userId) && !this.contentOwnershipService.IsOperator(userId))
            {
                return new HttpUnauthorizedResult(T("The given user doesn't have customer permission").Text);
            }

            var projects = this.projectService.GetProjects(null);
            var temp = this.projectService.GetUsersProjects(new[] { userId });
            List<ProjectPart> userProjects = new List<ProjectPart>();
            if (temp.ContainsKey(userId))
            {
                userProjects.AddRange(temp[userId]);
            }

            projects = projects.Where(c => !userProjects.Any(d => d.Record.Id == c.Id)).ToList();

            InviteUserToProjectViewModel model = new InviteUserToProjectViewModel();
            model.User = ProjectHelper.Convert<UserViewModel>(user);
            model.User.UserProjects.AddRange(userProjects.Select(c => new SelectListItem
            {
                Value = c.Record.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Record.Title
            }));

            model.Projects.AddRange(projects.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.As<ProjectPart>().Record.Title
            }));

            return this.View(model);
        }

        [HttpPost]
        public ActionResult InvitePost(InviteUserToProjectPostViewModel model)
        {
            if (model == null)
            {
                return HttpNotFound("There is no userId in the request");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = this.services.ContentManager.Get<IUser>(model.UserId);

            if (user == null)
            {
                return new HttpNotFoundResult(T("There is no user with the given Id").Text);
            }

            if (!this.contentOwnershipService.IsCustomer(model.UserId) && !this.contentOwnershipService.IsOperator(model.UserId))
            {
                return new HttpUnauthorizedResult(T("The given user doesn't have customer permission").Text);
            }

            EditContentPermissionViewModel dataModel = new EditContentPermissionViewModel
            {
                AccessType = ContentItemPermissionAccessTypes.SharedForView,
                ContentIds = model.Projects,
                RemoveOldPermission = false                
            };

            dataModel.Targets.Add(new TargetContentItemPermissionViewModel { UserId = model.UserId, Checked = true });

            var contentManager = this.services.ContentManager;
            var projects = contentManager.GetMany<ProjectPart>(dataModel.ContentIds, VersionOptions.Published, QueryHints.Empty).ToList();

            if (projects.Count() == 0)
            {
                throw new ArgumentNullException("There is no contentItem with the given Ids");
            }

            if (projects.Count() != dataModel.ContentIds.Length)
            {
                throw new ArgumentNullException("There is not a project with the given Id");
            }

            var projectsAsContentItems = projects.Select(c => c.ContentItem).ToList();
            if (this.contentOwnershipHelper.IsChangingPermissionsValid(dataModel, projectsAsContentItems, this.ModelState))
            {
                this.contentOwnershipHelper.Update(dataModel, projectsAsContentItems);
              
                foreach (var contentItem in projectsAsContentItems)
                {
                    if (this.masterDetailPermissionManager.HasChildItems(contentItem) && this.contentOwnershipService.IsOperator(model.UserId))
                    {
                        this.masterDetailPermissionManager.GrantPermissionToChildren(dataModel, contentItem);
                    }

                    var documentIndex = this.indexProvider.New(contentItem.Id);
                    contentManager.Index(contentItem, documentIndex);
                    this.indexProvider.Store(TicketController.SearchIndexName, documentIndex);
                }
            }

            return RedirectToAction("Invite", new { userId = model.UserId });
        }
    }
}