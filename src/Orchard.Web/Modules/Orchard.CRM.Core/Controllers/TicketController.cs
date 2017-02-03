using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.ActivityStream;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.CRM.Core.Controllers
{
    [Themed()]
    [ValidateInput(false)]
    public class TicketController : BaseController
    {
        public const string SearchIndexName = "CRMIndex";
        private readonly ISiteService siteService;
        private readonly IContentDefinitionManager contentDefinitionManager;
        protected IRepository<UserRolesPartRecord> userRolesRepository;
        private readonly IBusinessUnitService businessUnitService;
        private readonly IIndexManager indexManager;
        private IRepository<BusinessUnitMemberPartRecord> businessUnitMembersRepository;
        private readonly ISearchTicketService searchTicketService;
        private readonly IWorkContextAccessor workContextAccessor;
        private readonly IProjectService projectService;
        private readonly Lazy<IEnumerable<IContentHandler>> handlers;

        public TicketController(
            Lazy<IEnumerable<IContentHandler>> handlers,
            IProjectService projectService,
            IIndexProvider indexProvider,
            ISearchTicketService searchTicketService,
            IContentOwnershipHelper contentOwnershipHelper,
            IRepository<UserRolesPartRecord> userRolesRepository,
            ICRMContentOwnershipService crmContentOwnershipService,
            IExtendedContentManager extendedContentManager,
            ITransactionManager transactionManager,
            IWidgetService widgetService,
            IThemeAwareViewEngine themeAwareViewEngine,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IOrchardServices services,
            IDisplayHelperFactory displayHelperFactory,
            IBusinessUnitService businessUnitService,
            ISiteService siteService,
            IBasicDataService basicDataService,
            IContentDefinitionManager contentDefinitionManager,
            IRepository<BusinessUnitMemberPartRecord> businessUnitMembersRepository,
            IIndexManager indexManager,
            IWorkContextAccessor workContextAccessor,
            IActivityStreamService streamService,
            IContentItemDescriptorManager contentItemDescriptorManager)
            : base("Ticket", "Ticket_Edit", indexProvider, services, crmContentOwnershipService, transactionManager, extendedContentManager, contentManager, widgetService, themeAwareViewEngine, shapeFactory, displayHelperFactory, basicDataService, contentOwnershipHelper, streamService, contentItemDescriptorManager)
        {
            this.handlers = handlers;
            this.projectService = projectService;
            this.workContextAccessor = workContextAccessor;
            this.searchTicketService = searchTicketService;
            this.userRolesRepository = userRolesRepository;
            this.businessUnitService = businessUnitService;
            this.siteService = siteService;
            this.contentDefinitionManager = contentDefinitionManager;
            this.indexManager = indexManager;
            this.businessUnitMembersRepository = businessUnitMembersRepository;
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
            this.defaultQueryHint = this.defaultQueryHint.ExpandRecords<TicketPartRecord>();
            this.defaultQueryHint = this.defaultQueryHint.ExpandRecords<ContentItemPermissionPartRecord>();
            this.defaultQueryHint = this.defaultQueryHint.ExpandRecords<CRMCommentsPartRecord>();
        }

        public IEnumerable<IContentHandler> Handlers
        {
            get { return this.handlers.Value; }
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        /// <summary>
        /// One of the reasons we have a different Action for creating a child ticket, is setting the properties related to the Parent ticket such as
        /// Parent_Id and AttachToProjectId for the child ticket
        /// </summary>
        /// <param name="parentTicketId"></param>
        /// <returns></returns>
        public ActionResult CreateChildTicket(int parentTicketId)
        {
            var contentItem = this.contentManager.New(this.ControllerContentType);

            var parentTicketContentItem = this.contentManager.Get(parentTicketId, VersionOptions.Published, new QueryHints().ExpandParts<TicketPart, AttachToProjectPart>());

            if (parentTicketContentItem != null &&
                parentTicketContentItem.ContentType == "Ticket" &&
                this.crmContentOwnershipService.CurrentUserCanViewContent(parentTicketContentItem))
            {
                TicketPart ticket = contentItem.As<TicketPart>();
                TicketPart parentTicket = parentTicketContentItem.As<TicketPart>();
                ticket.Record.Parent = parentTicket.Record;

                AttachToProjectPart attachToProjectPart = contentItem.As<AttachToProjectPart>();
                if (attachToProjectPart != null)
                {
                    var parentAttachToProject = parentTicketContentItem.As<AttachToProjectPart>();
                    if (parentAttachToProject.Record.Project != null)
                    {
                        attachToProjectPart.Record.Project = new ProjectPartRecord
                        {
                            Id = parentAttachToProject.Record.Project.Id
                        };
                    }
                }
            }

            if (!this.crmContentOwnershipService.IsCurrentUserOperator())
                return new HttpUnauthorizedResult();

            dynamic model = this.contentManager.BuildEditor(contentItem);

            if (!string.IsNullOrEmpty(this.EditMetadataType))
            {
                model.Metadata.Type = this.EditMetadataType;
            }

            this.OnCreated(model);

            return this.CreateActionResultBasedOnAjaxRequest(model, "Create", "Create", string.Empty);
        }

        public ActionResult Search(PagerParametersWithSortFields pagerParameters, PostedTicketSearchViewModel searchModel)
        {
            if (!this.IsDisplayAuthorized())
            {
                return new HttpUnauthorizedResult();
            }

            // A simple solution for the bug of sending page paraemter via querystring, if searchModel has value, with unknown reason, the page will not be set
            if (pagerParameters != null && pagerParameters.Page == null && !string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                int page;
                if (int.TryParse(Request.QueryString["page"], out page))
                {
                    pagerParameters.Page = page;
                }
            }

            if (this.crmContentOwnershipService.IsCurrentUserCustomer())
            {
                searchModel.Unassigned = false;
                searchModel.Users = new int[] { };
                searchModel.IncludeAllVisibleItemsBySelectedGroupsAndUsers = false;
            }

            // add default sort field, if it is not provided
            if (string.IsNullOrEmpty(pagerParameters.SortField))
            {
                pagerParameters.SortField = TicketPart.IdentityFieldName;
                pagerParameters.Descending = true;
            }

            if (searchModel != null)
            {
                searchModel.Users = searchModel.Users ?? new int[] { };
                searchModel.BusinessUnits = searchModel.BusinessUnits ?? new int[] { };
            }

            if (!string.IsNullOrEmpty(searchModel.Status) && !this.basicDataService.GetStatusRecords().Any(c => c.Id.ToString() == searchModel.Status))
            {
                searchModel.Status = string.Empty;
            }

            // full text search will be done by lucene
            if (string.IsNullOrEmpty(searchModel.Term))
            {
                return this.SearchByHqlQuery(pagerParameters, searchModel);
            }
            else
            {
                return this.SearchByLucene(pagerParameters, searchModel);
            }
        }

        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult QuickUpdate(TicketQuickUpdateViewModel model, string displyType)
        {
            if (!this.crmContentOwnershipService.IsCurrentUserOperator() && !this.crmContentOwnershipService.IsCurrentUserCustomer())
                return new HttpUnauthorizedResult();

            if (model.Ids.Count == 0)
            {
                this.ModelState.AddModelError("Ids", "The model must contains minimum one contentItem.Id");
            }

            var tickets = this.contentManager.GetMany<TicketPart>(model.Ids, VersionOptions.DraftRequired, new QueryHints().ExpandRecords(new[] { "TicketPartRecord", "ContentItemPermissionPartRecord" })).ToList();

            if (tickets.Count != model.Ids.Count)
            {
                return HttpNotFound();
            }

            bool afterUpdateUserStillCanChangeOwnership = true;
            if (ModelState.IsValid)
            {
                foreach (var ticket in tickets)
                {
                    var contentItem = ticket.ContentItem;

                    var context = new UpdateContentContext(contentItem);

                    this.Handlers.Invoke(handler => handler.Updating(context), Logger);

                    if (contentItem == null)
                        return HttpNotFound();

                    if (!this.IsEditAuthorized(contentItem))
                    {
                        this.ModelState.AddModelError("Ids", T("You don't have right access to change these items").Text);
                        return this.CreateActionResultBasedOnAjaxRequest(null, null);
                    }

                    dynamic snapshot = this.streamService.TakeSnapshot(contentItem);

                    if (model.UpdateBusinessUnitId || model.UpdateTeamId || model.UpdateUserId)
                    {
                        PostedEditOwnerViewModel postedEditOwnerViewModel = new PostedEditOwnerViewModel();
                        postedEditOwnerViewModel.UserId = model.UpdateUserId ? model.UserId : null;

                        if (model.BusinessUnitId.HasValue)
                        {
                            postedEditOwnerViewModel.GroupId = model.UpdateBusinessUnitId ?
                                "BusinessUnit:" + model.BusinessUnitId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                        }

                        if (model.UpdateTeamId && model.TeamId.HasValue)
                        {
                            postedEditOwnerViewModel.GroupId = "Team:" + model.TeamId.Value.ToString(CultureInfo.InvariantCulture);
                        }

                        this.EditOwner(contentItem, postedEditOwnerViewModel, false, false);
                        afterUpdateUserStillCanChangeOwnership = this.crmContentOwnershipService.CurrentUserIsContentItemAssignee(contentItem) || this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);
                    }

                    TicketPart ticketPart = contentItem.As<TicketPart>();

                    // Title
                    if (!string.IsNullOrEmpty(model.Title))
                    {
                        ticketPart.Record.Title = model.Title;
                    }

                    // Description
                    if (model.UpdateDescription)
                    {
                        ticketPart.Record.Description = model.Description;
                    }

                    // Priority
                    if (model.PriorityId.HasValue)
                    {
                        ticketPart.Record.PriorityRecord = new PriorityRecord { Id = model.PriorityId.Value };
                    }
                    else if (model.UpdatePriority)
                    {
                        ticketPart.Record.PriorityRecord = null;
                    }

                    // serviceId
                    if (model.ServiceId.HasValue)
                    {
                        ticketPart.Record.Service = new ServicePartRecord { Id = model.ServiceId.Value };
                    }
                    else if (model.UpdateServiceId)
                    {
                        ticketPart.Record.Service = null;
                    }

                    // statusId
                    if (model.StatusId.HasValue)
                    {
                        ticketPart.Record.StatusRecord = new StatusRecord { Id = model.StatusId.Value };
                    }
                    else if (model.UpdateStatusId)
                    {
                        ticketPart.Record.StatusRecord = null;
                    }

                    // TicketTypeId
                    if (model.TypeId.HasValue)
                    {
                        ticketPart.Record.TicketType = new TicketTypeRecord { Id = model.TypeId.Value };
                    }
                    else if (model.UpdateTypeId)
                    {
                        ticketPart.Record.TicketType = null;
                    }

                    // DueDate
                    if (model.DueDate.HasValue)
                    {
                        ticketPart.Record.DueDate = model.DueDate.Value;
                    }
                    else if (model.UpdateDueDate)
                    {
                        ticketPart.Record.DueDate = null;
                    }

                    this.Handlers.Invoke(handler => handler.Updated(context), Logger);

                    contentManager.Publish(contentItem);

                    this.streamService.WriteChangesToStreamActivity(contentItem, snapshot, StreamWriters.TicketStreamWriter);
                    var documentIndex = this.indexProvider.New(contentItem.Id);
                    this.contentManager.Index(contentItem, documentIndex);
                    this.indexProvider.Store(TicketController.SearchIndexName, documentIndex);
                }
            }

            bool isAjaxRequest = Request.IsAjaxRequest();

            if (isAjaxRequest)
            {
                displyType = string.IsNullOrEmpty(displyType) ? "Summary" : displyType;
                List<dynamic> shapes = new List<dynamic>();

                foreach (var contentItem in tickets)
                {
                    var shape = this.contentManager.BuildDisplay(tickets[0].ContentItem, displayType: displyType);
                    shapes.Add(shape);
                }

                var data = new
                {
                    Tickets = Newtonsoft.Json.JsonConvert.SerializeObject(shapes),
                    ChangeOwnershipIsPossible = afterUpdateUserStillCanChangeOwnership
                };

                AjaxMessageViewModel ajaxMessageModel = new AjaxMessageViewModel { Id = tickets[0].ContentItem.Id, IsDone = ModelState.IsValid, Data = data };

                foreach (var item in ModelState)
                {
                    ajaxMessageModel.Errors.AddRange(item.Value.Errors.Select(c => new KeyValuePair<string, string>(item.Key, c.Exception != null ? c.Exception.Message : c.ErrorMessage)).Where(c => !string.IsNullOrEmpty(c.Value)));
                }

                return this.Json(ajaxMessageModel, JsonRequestBehavior.AllowGet);
            }
            else if (!string.IsNullOrEmpty(model.returnUrl))
            {
                return this.Redirect(model.returnUrl);
            }
            else
            {
                return this.RedirectToAction("Edit", new RouteValueDictionary { { "Id", tickets[0].ContentItem.Id } });
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var wc = this.workContextAccessor.GetContext();

            // rendering default layout in case notheme flag is true
            var ticketSettings = this.services.WorkContext.CurrentSite.As<TicketSettingPart>();
            if (ticketSettings.WithoutTheme)
            {
                wc.Layout.Metadata.Alternates.Add("CRMLayout");
            }

            wc.Layout.TicketRelatedPage = true;
        }

        protected override bool IsCreateAuthorized()
        {
            return this.crmContentOwnershipService.IsCurrentUserOperator() || this.crmContentOwnershipService.IsCurrentUserCustomer();
        }

        protected override bool IsDisplayAuthorized()
        {
            return this.services.Authorizer.Authorize(Permissions.OperatorPermission) ||
                this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission) ||
                this.services.Authorizer.Authorize(Permissions.CustomerPermission);
        }

        protected override void OnCreating(ContentItem contentItem)
        {
            CreateModel createModel = new CreateModel();
            this.TryUpdateModel(createModel, "TicketPart");

            if (createModel.RelatedContentItemId.HasValue)
            {
                TicketPart ticketPart = contentItem.As<TicketPart>();

                if (ticketPart == null)
                {
                    return;
                }

                ticketPart.Record.RelatedContentItem = new ContentManagement.Records.ContentItemRecord { Id = createModel.RelatedContentItemId.Value };
            }

            base.OnCreating(contentItem);
        }

        protected override void OnCreatingPost(ContentItem contentItem)
        {
            // give edit permission to current user in case current user has no permission
            var contentPermissionPart = contentItem.As<ContentItemPermissionPart>();
            if (contentPermissionPart != null)
            {
                int userId = this.services.WorkContext.CurrentUser.Id;
                var userPrermissions = this.contentOwnershipHelper.GetUserPermissionRecordsForItem(contentItem, userId);
                if (userPrermissions == null || userPrermissions.Count() == 0)
                {
                    byte accessType = ContentItemPermissionAccessTypes.SharedForEdit;
                    this.contentOwnershipHelper.Create(
                        new ContentItemPermissionDetailRecord
                        {
                            AccessType = accessType,
                            User = new Users.Models.UserPartRecord { Id = userId }
                        },
                        contentItem
                        , false);
                }
            }
        }

        protected override void OnCreatePost(ContentItem contentItem)
        {
            bool isCustomer = this.crmContentOwnershipService.IsCurrentUserCustomer();

            if (isCustomer)
            {
                var ticketSettings = this.services.WorkContext.CurrentSite.As<TicketSettingPart>();
                var ticket = contentItem.As<TicketPart>();

                int statusId = ticketSettings.CreatedCustomerTicketState;
                if (statusId == default(int))
                {
                    var statusRecords = this.basicDataService.GetStatusRecords().ToList();
                    var newStatus = statusRecords.FirstOrDefault(c => c.StatusTypeId == StatusRecord.NewStatus);
                    statusId = newStatus.Id;
                }

                ticket.Record.StatusRecord = new StatusRecord { Id = statusId };
                ticket.Record.DueDate = DateTime.UtcNow.AddDays(ticketSettings.CustomerTicketDueDateDays);
            }

            base.OnCreatePost(contentItem);
        }

        protected override void OnDisplay(ContentItem contentItem, dynamic model)
        {
            TicketPart ticketPart = contentItem.As<TicketPart>();

            if (ticketPart.Record.RelatedContentItem != null)
            {
                var relatedContentItem = this.contentManager.Get(ticketPart.Record.RelatedContentItem.Id, VersionOptions.AllVersions);
                TitlePart titlePart = relatedContentItem.As<TitlePart>();
                CommonPart commonPart = relatedContentItem.As<CommonPart>();

                if (titlePart != null && commonPart != null)
                {
                    model.RelatedContentItem = new
                    {
                        Owner = commonPart.Owner,
                        Title = titlePart.Title,
                        Id = relatedContentItem.Id,
                        ContentType = relatedContentItem.ContentType
                    };
                }
            }

            model.IsCurrentUserCustomer = this.crmContentOwnershipService.IsCurrentUserCustomer();
            model.CurrentUserCanEditContent = this.crmContentOwnershipService.CurrentUserCanEditContent(contentItem);
            model.CurrentUserCanChangePermission = this.crmContentOwnershipService.CurrentUserCanChangePermission(contentItem, new ModelStateDictionary());
        }

        private ActionResult SearchByLucene(PagerParametersWithSortFields pagerParameters, PostedTicketSearchViewModel searchModel)
        {
            Pager pager = new Pager(siteService.GetSiteSettings(), pagerParameters);

            if (!indexManager.HasIndexProvider())
            {
                return View("NoIndex");
            }

            // Status contentType
            int totalCount = this.searchTicketService.CountByIndexProvider(searchModel);
            var contentItems = this.searchTicketService.SearchByIndexProvider(pagerParameters, searchModel);
            SearchTicketsViewModel model = this.CreateSearchModel(contentItems, pager, searchModel, pagerParameters, totalCount);

            return this.View(model);
        }

        private ActionResult SearchByHqlQuery(PagerParametersWithSortFields pagerParameters, PostedTicketSearchViewModel searchModel)
        {
            Pager pager = new Pager(siteService.GetSiteSettings(), pagerParameters);
            int totalCount = this.searchTicketService.CountByDatabase(searchModel);
            var contentItems = this.searchTicketService.SearchByDatabase(pagerParameters, searchModel);

            SearchTicketsViewModel model = this.CreateSearchModel(contentItems, pager, searchModel, pagerParameters, totalCount);

            return this.View(model);
        }

        private SearchTicketsViewModel CreateSearchModel(IEnumerable<IContent> contentItems, Pager pager, PostedTicketSearchViewModel postedSearchModel, PagerParametersWithSortFields pagerParameters, int totalCount)
        {
            SearchTicketsViewModel model = new SearchTicketsViewModel();
            model.Term = postedSearchModel.Term;
            model.IsAdminUser = this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);
            if (postedSearchModel.DueDate == PostedTicketSearchViewModel.OverDueDate)
            {
                model.Overdue = true;
            }
            else
            {
                model.Overdue = false;
                DateTime value;
                if (DateTime.TryParse(postedSearchModel.DueDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out value))
                {
                    model.DueDate = value;
                }
            }

            model.StatusId = postedSearchModel.Status;
            model.UnStatus = postedSearchModel.UnStatus;
            model.PagerParameters = pagerParameters;
            model.RelatedContentItemId = postedSearchModel.RelatedContentItemId;

            // related contentItem
            if (postedSearchModel.RelatedContentItemId.HasValue)
            {
                var relatedContentItem = this.contentManager.Get(postedSearchModel.RelatedContentItemId.Value);
                var titlePart = relatedContentItem.As<TitlePart>();
                if (titlePart != null)
                {
                    model.RelatedContentItemTitle = titlePart.Title;
                }
            }

            model.Pager = this.services.New.Pager(pager).TotalItemCount(totalCount);

            model.Items = new List<dynamic>();
            foreach (var contentItem in contentItems)
            {
                // ignore search results which content item has been removed or unpublished
                if (contentItem == null)
                {
                    totalCount--;
                    continue;
                }

                var itemModel = this.contentManager.BuildDisplay(contentItem, "TableRow");
                itemModel.Metadata.Type = "Ticket_TableRow_Container";
                model.Items.Add(itemModel);
                itemModel.IsEditable = this.crmContentOwnershipService.CurrentUserCanEditContent(contentItem);
            }

            model.Unassigned = postedSearchModel.Unassigned;
            this.FillBusinessUnitsAndUsers(model, postedSearchModel);

            // Projects
            if (this.projectService.IsTicketsRelatedToProjects())
            {
                model.IsProjectForTicketsSupported = true;
                model.ProjectId = postedSearchModel.ProjectId;
                var projects = this.projectService.GetProjects(null);
                Converter.Fill(model.Projects, projects.AsPart<ProjectPart>());
            }

            if (this.crmContentOwnershipService.IsCurrentUserCustomer())
            {
                model.IsCustomerUser = true;
                model.Users.Clear();
                model.BusinessUnits.ToList().ForEach(c => c.Teams.Clear());
            }

            var statusRecords = this.basicDataService.GetStatusRecords().ToList();
            model.ClosedStatusId = statusRecords.First(c => c.StatusTypeId == StatusRecord.ClosedStatus).Id;
            model.OpenStatusId = statusRecords.First(c => c.StatusTypeId == StatusRecord.OpenStatus).Id;

            // IncludeAllVisibleItemsBySelectedGroupsAndUsers  is meaningful, if there is a selected user or business unit
            model.IncludeAllVisibleItemsBySelectedGroupsAndUsers = postedSearchModel.IncludeAllVisibleItemsBySelectedGroupsAndUsers &&
                (model.Users.Any(c => c.Checked) || model.BusinessUnits.Any(c => c.Checked) || model.BusinessUnits.SelectMany(c => c.Teams).Any(c => c.Checked));

            model.SearchDescription = this.GetSearchDescription(model, statusRecords);

            return model;
        }

        private string GetSearchDescription(SearchTicketsViewModel model, IEnumerable<StatusRecord> statusList)
        {
            List<string> parts = new List<string>();

            string format = "<span class='label'>{0}</span>: {1}";
            string oneParameterFormat = "<span class='label'>{0}</span>";

            // groups
            var selectedBusinessUnits = model.BusinessUnits.Where(c => c.Checked);
            var selectedTeams = model.BusinessUnits.SelectMany(c => c.Teams.Where(d => d.Checked));
            var groups = selectedBusinessUnits.Select(c => c.Name).Union(selectedTeams.Select(d => d.Name));
            if (groups.Any())
            {
                string groupsString = string.Format(CultureInfo.CurrentUICulture, format, T("Groups").Text, string.Join(", ", groups));
                parts.Add(groupsString);
            }

            // project
            if (model.ProjectId.HasValue && this.projectService.IsTicketsRelatedToProjects())
            {
                var project = this.projectService.GetProject(model.ProjectId.Value);
                string projectName = project != null ? project.Record.Title : this.T("UNKNOWN Project").Text;
                string projectNameString = string.Format(CultureInfo.CurrentUICulture, format, T("Project").Text, projectName);
                parts.Add(projectNameString);
            }

            // related ContentItem
            if (model.RelatedContentItemId.HasValue)
            {
                if (!string.IsNullOrEmpty(model.RelatedContentItemTitle))
                {
                    string text = string.Format(CultureInfo.CurrentUICulture, format, T("Tickets related to").Text, model.RelatedContentItemTitle);
                    parts.Add(text);
                }
                else
                {
                    string text = string.Format(CultureInfo.CurrentUICulture, format, T("Tickets related to ContentId").Text, model.RelatedContentItemId.Value.ToString(CultureInfo.InvariantCulture));
                    parts.Add(text);
                }
            }

            // users
            var selectedUsers = model.Users.Where(c => c.Checked).Select(c => c.Username);
            if (selectedUsers.Any())
            {
                string selectedUsersString = string.Format(CultureInfo.CurrentUICulture, format, T("Users").Text, string.Join(", ", selectedUsers));
                parts.Add(selectedUsersString);
            }

            // Unassigned
            if (model.Unassigned)
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, T("Unassigned").Text));
            }

            // Status
            if (!string.IsNullOrEmpty(model.StatusId))
            {
                var status = statusList.FirstOrDefault(c => c.Id.ToString(CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture) == model.StatusId.ToUpper(CultureInfo.InvariantCulture));
                if (status != null)
                {
                    parts.Add(string.Format(CultureInfo.CurrentUICulture, format, T("Status").Text, status.Name));
                }
            }

            // Unstatus
            if (model.UnStatus)
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, T("No Status").Text));
            }

            // Due date
            if (model.DueDate.HasValue)
            {
                if (model.DueDate.Value.Date > DateTime.UtcNow)
                {
                    parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, T("Not Overdue").Text));
                }

                parts.Add(string.Format(CultureInfo.CurrentUICulture, format, T("Due Date").Text, model.DueDate.Value.ToString("yyyy/MM/dd")));
            }

            // Overdue
            if (model.Overdue)
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, T("Overdue").Text));
            }

            // IncludeAllVisibleItemsBySelectedGroupsAndUsers
            if (model.IncludeAllVisibleItemsBySelectedGroupsAndUsers)
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, this.T("All visible Tickets by the selected agents or groups").Text));
            }

            // Term
            if (!string.IsNullOrEmpty(model.Term))
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, format, T("Term").Text, model.Term));
            }

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Fills the model with the businessUnits that the user has granted to view them
        /// </summary>
        private void FillBusinessUnitsAndUsers(SearchTicketsViewModel model, PostedTicketSearchViewModel postedSearchModel)
        {
            bool restrictToUserPermissions = !this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);
            this.businessUnitService.Fill(model.BusinessUnits, restrictToUserPermissions);

            var selectedBusinessUnits = postedSearchModel.BusinessUnits.ToList();

            // TeamIds of the search
            var teams = new List<int>();

            // set checkes of businessUnits
            model.BusinessUnits
                .ToList()
                .ForEach(c => c.Checked = selectedBusinessUnits.Count(d => d == c.BusinessUnitId) > 0);

            // set checks of teams
            model.BusinessUnits
                .SelectMany(c => c.Teams)
                .ToList()
                .ForEach(c => c.Checked = teams.Count(d => d == c.TeamId) > 0);

            IEnumerable<IUser> users = null;

            users = this.basicDataService.GetOperators().ToList();

            foreach (var user in users)
            {
                SearchTicketsViewModel.UserViewModel userViewModel = new SearchTicketsViewModel.UserViewModel
                {
                    Id = user.Id,
                    Username = CRMHelper.GetFullNameOfUser(user.As<UserPart>()),
                    Checked = postedSearchModel.Users.Count(c => c == user.Id) > 0,
                    IsAdminOrOperator = true
                };

                model.Users.Add(userViewModel);
            }
        }

        /// <summary>
        /// This model is used in rendering the create ticket page.
        /// </summary>
        private class CreateModel
        {
            public int? RelatedContentItemId { get; set; }
        }
    }
}