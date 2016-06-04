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

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMSyncService : ISuiteCRMSyncService
    {
        private readonly ISuiteCRMDataService suiteCRMProjectService;
        private readonly IOrchardServices services;
        private readonly ISearchTicketService searchTicketService;
        private readonly IExtendedProjectService projectService;
        private readonly IRepository<PriorityRecord> priorityRepository;
        private readonly IRepository<StatusRecord> statusRepository;
        private readonly IActivityStreamService streamService;

        public const string UsersBean = "Users";

        public SuiteCRMSyncService(
            IActivityStreamService streamService,
            IRepository<PriorityRecord> priorityRepository,
            IRepository<StatusRecord> statusRepository,
            ISuiteCRMDataService suiteCRMProjectService,
            IOrchardServices services,
            ISearchTicketService searchTicketService,
            IExtendedProjectService projectService)
        {
            this.streamService = streamService;
            this.priorityRepository = priorityRepository;
            this.statusRepository = statusRepository;
            this.projectService = projectService;
            this.searchTicketService = searchTicketService;
            this.suiteCRMProjectService = suiteCRMProjectService;
            this.services = services;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<SuiteCRMProjectDetailViewModel> GetProjects(int pageNumber, int pageSize, bool basedOnSuiteCRMList)
        {
            IEnumerable<project> suiteCRMProjects = null;
            IEnumerable<SuiteCRMProjectPart> orchardCollaborationProjects = null;

            if (basedOnSuiteCRMList)
            {
                suiteCRMProjects = this.suiteCRMProjectService.GetProjects(pageNumber, pageSize);

                // TODO: it gets the list of all projects. Find a better way
                orchardCollaborationProjects = this.projectService.GetProjects(null).Where(c => c.Is<SuiteCRMProjectPart>()).Select(c => c.As<SuiteCRMProjectPart>()).ToList();
                orchardCollaborationProjects = orchardCollaborationProjects.Where(c => suiteCRMProjects.Any(d => !string.IsNullOrEmpty(c.ExternalId) && d.id.ToLower() == c.ExternalId.ToLower())).ToList();
            }
            else
            {
                var pager = new Pager(this.services.WorkContext.CurrentSite, pageNumber + 1, pageSize);
                orchardCollaborationProjects = this.projectService.GetProjects(pager).Where(c => c.Is<SuiteCRMProjectPart>()).Select(c => c.As<SuiteCRMProjectPart>()).ToList();

                var suiteCRMIds = orchardCollaborationProjects.Select(c => c.ExternalId).ToArray();
                suiteCRMProjects = this.suiteCRMProjectService.GetProjects(suiteCRMIds);
            }

            List<SuiteCRMProjectDetailViewModel> returnValue = new List<SuiteCRMProjectDetailViewModel>();

            foreach (var project in orchardCollaborationProjects.OrderByDescending(c => c.As<CommonPart>().ModifiedUtc))
            {
                SuiteCRMProjectDetailViewModel projectModel = new SuiteCRMProjectDetailViewModel();
                projectModel.OrchardCollaborationProject = project.ContentItem;
                projectModel.LastSyncTime = project.LastSyncTime;

                ProjectPart projectPart = project.As<ProjectPart>();
                if (projectPart != null)
                {
                    projectModel.OrchardCollaborationTitle = projectPart.Title;
                }

                if (!string.IsNullOrEmpty(project.ExternalId))
                {
                    var suiteCRMPRoject = suiteCRMProjects.FirstOrDefault(c => c.id.ToLower() == project.ExternalId.ToLower());

                    if (suiteCRMPRoject != null)
                    {
                        projectModel.IsSync = true;
                        projectModel.SuiteCRMProject = this.Convert(suiteCRMPRoject);
                    }
                }

                returnValue.Add(projectModel);
            }

            foreach (var suiteCRMProject in suiteCRMProjects.Where(c => !returnValue.Any(d => d.SuiteCRMProject != null && d.SuiteCRMProject.Id == c.id)))
            {
                SuiteCRMProjectDetailViewModel projectModel = new SuiteCRMProjectDetailViewModel();
                projectModel.SuiteCRMProject = this.Convert(suiteCRMProject);
                returnValue.Add(projectModel);
            }

            return returnValue;
        }

        public IEnumerable<SuiteCRMProjectDetailViewModel> CopySuiteCRMProjectsToOrchard(CopyOrchardProjectToSuiteViewModel model)
        {
            List<SuiteCRMProjectDetailViewModel> returnValue = new List<SuiteCRMProjectDetailViewModel>();

            using (var connection = Helper.GetConnection(this.services, this.Logger))
            using (SuiteCRMProjectUnitOfWork projectRepository = new SuiteCRMProjectUnitOfWork(connection))
            using (SuiteCRMProjectTaskUnitOfWork projectTasksRepository = new SuiteCRMProjectTaskUnitOfWork(projectRepository))
            {
                var suiteProjects = projectRepository.GetProjects(model.Projects.Where(c => !string.IsNullOrEmpty(c.SuiteCRMId)).Select(c => c.SuiteCRMId).ToArray());
                var orchardProjects = this.services
                                          .ContentManager
                                          .GetMany<SuiteCRMProjectPart>(
                                            model.Projects.Where(c => c.OrchardCollaborationProjectId.HasValue).Select(c => c.OrchardCollaborationProjectId.Value),
                                            VersionOptions.Published,
                                            new QueryHints().ExpandParts<ProjectPart>());

                foreach (var item in model.Projects.Where(c => !string.IsNullOrEmpty(c.SuiteCRMId)))
                {
                    var suiteCRMProject = suiteProjects.FirstOrDefault(c => c.id.ToLower() == item.SuiteCRMId.ToLower());

                    if (suiteCRMProject == null)
                    {
                        continue;
                    }

                    ContentItem orchardProject = null;
                    dynamic projectSnapshot = null;
                    bool isNew = false;
                    if (item.OrchardCollaborationProjectId.HasValue)
                    {
                        var part = orchardProjects.FirstOrDefault(c => c.Id == item.OrchardCollaborationProjectId.Value);
                        if (part != null)
                        {
                            orchardProject = part.ContentItem;
                            projectSnapshot = this.streamService.TakeSnapshot(orchardProject);
                        }
                        else
                        {
                            isNew = true;
                            orchardProject = this.services.ContentManager.New("ProjectItem");
                        }
                    }
                    else
                    {
                        isNew = true;
                        orchardProject = this.services.ContentManager.New("ProjectItem");
                    }

                    if (isNew)
                    {
                        ProjectDashboardEditorPart editorPart = orchardProject.As<ProjectDashboardEditorPart>();
                        var portletTemplates = this.projectService.GetPortletsTemplates();
                        editorPart.PortletList = this.projectService.GetDefaultPortletIds(portletTemplates).ToArray();
                        this.services.ContentManager.Create(orchardProject);
                    }

                    // by building editor, we can be sure that the default values will be set
                    this.services.ContentManager.BuildEditor(orchardProject);

                    SuiteCRMProjectPart suiteCRMProjectPart = orchardProject.As<SuiteCRMProjectPart>();
                    ProjectPart projectPart = orchardProject.As<ProjectPart>();
                    suiteCRMProjectPart.ExternalId = suiteCRMProject.id;
                    suiteCRMProjectPart.LastSyncTime = DateTime.UtcNow;

                    // the values will be overridde in case user doesn't care about update time (item.DoNotOverrideNewerValues == false) or
                    // the target modified date is less than source modified date
                    DateTime? lastSuiteCRMChangeDate = suiteCRMProject.date_modified ?? suiteCRMProject.date_entered;
                    CommonPart commonPart = orchardProject.As<CommonPart>();
                    if (!item.DoNotOverrideNewerValues ||
                        isNew ||
                        (lastSuiteCRMChangeDate.HasValue && commonPart.ModifiedUtc <= lastSuiteCRMChangeDate.Value))
                    {
                        this.Copy(suiteCRMProject, projectPart);
                        this.services.ContentManager.Publish(orchardProject);
                        item.OrchardCollaborationProjectId = orchardProject.Id;
                        this.streamService.WriteChangesToStreamActivity(orchardProject, projectSnapshot, null);
                    }

                    if (item.SyncTasks)
                    {
                        TicketContext context = new TicketContext();
                        context.ProjectTaskUnitOfWork = projectTasksRepository;
                        context.Priorities = this.priorityRepository.Table.ToList();
                        context.StatusList = this.statusRepository.Table.ToList();
                        this.CopySuiteCRMTasksToOrchardTickets(context, item);
                    }

                    SuiteCRMProjectDetailViewModel projectModel = new SuiteCRMProjectDetailViewModel();
                    projectModel.SuiteCRMProject = this.Convert(suiteCRMProject);
                    projectModel.LastSyncTime = suiteCRMProjectPart.LastSyncTime;
                    projectModel.OrchardCollaborationProject = orchardProject;
                    projectModel.OrchardCollaborationTitle = projectPart.Title;

                    returnValue.Add(projectModel);
                }

            }

            return returnValue;
        }

        public IEnumerable<SuiteCRMProjectDetailViewModel> CopyOrchardProjectsToSuite(CopyOrchardProjectToSuiteViewModel model)
        {
            List<SuiteCRMProjectDetailViewModel> returnValue = new List<SuiteCRMProjectDetailViewModel>();

            using (var connection = Helper.GetConnection(this.services, this.Logger))
            using (SuiteCRMProjectUnitOfWork projectRepository = new SuiteCRMProjectUnitOfWork(connection))
            using (SuiteCRMProjectTaskUnitOfWork projectTasksRepository = new SuiteCRMProjectTaskUnitOfWork(projectRepository))
            using (var suiteCRMTransaction = projectRepository.BeginTransaction())
            {
                try
                {
                    var suiteProjects = projectRepository.GetProjects(model.Projects.Where(c => !string.IsNullOrEmpty(c.SuiteCRMId)).Select(c => c.SuiteCRMId).ToArray());
                    var orchardProjects = this.services
                                              .ContentManager
                                              .GetMany<SuiteCRMProjectPart>(
                                                model.Projects.Where(c => c.OrchardCollaborationProjectId.HasValue).Select(c => c.OrchardCollaborationProjectId.Value),
                                                VersionOptions.Published,
                                                new QueryHints().ExpandParts<ProjectPart>());

                    foreach (var item in model.Projects.Where(c => c.OrchardCollaborationProjectId.HasValue))
                    {
                        var suiteCRMProjectPart = orchardProjects.FirstOrDefault(c => c.Id == item.OrchardCollaborationProjectId.Value);

                        if (suiteCRMProjectPart == null)
                        {
                            continue;
                        }

                        var orchardProject = suiteCRMProjectPart.As<ProjectPart>();

                        project suiteCRMProject = null;
                        if (!string.IsNullOrEmpty(item.SuiteCRMId))
                        {
                            suiteCRMProject = suiteProjects.FirstOrDefault(c => c.id == item.SuiteCRMId);

                            if (suiteCRMProject == null)
                            {
                                suiteCRMProject = new project();
                                projectRepository.Add(suiteCRMProject);
                            }
                        }
                        else
                        {
                            suiteCRMProject = new project();
                            projectRepository.Add(suiteCRMProject);
                        }

                        // the values will be overridde in case user doesn't care about update time (item.DoNotOverrideNewerValues == false) or
                        // the target modified date is smaller than source modified date
                        CommonPart commonPart = orchardProject.As<CommonPart>();
                        DateTime? lastOrchardTicketChangeDate = commonPart.ModifiedUtc ?? commonPart.CreatedUtc;
                        if (!item.DoNotOverrideNewerValues ||
                            suiteCRMProject.date_modified == null ||
                            (lastOrchardTicketChangeDate.HasValue &&
                            suiteCRMProject.date_modified.Value <= lastOrchardTicketChangeDate.Value))
                        {
                            this.Copy(orchardProject, suiteCRMProject);
                            projectRepository.Save();
                        }

                        suiteCRMProjectPart.LastSyncTime = DateTime.UtcNow;
                        suiteCRMProjectPart.ExternalId = suiteCRMProject.id;
                        item.SuiteCRMId = suiteCRMProject.id;

                        if (item.SyncTasks)
                        {
                            TicketContext context = new TicketContext();
                            context.ProjectTaskUnitOfWork = projectTasksRepository;
                            context.Priorities = this.priorityRepository.Table.ToList();
                            context.StatusList = this.statusRepository.Table.ToList();
                            this.CopyOrchardTicketsToSuiteCRM(context, item);
                        }

                        this.services.ContentManager.Publish(suiteCRMProjectPart.ContentItem);

                        SuiteCRMProjectDetailViewModel projectModel = new SuiteCRMProjectDetailViewModel();
                        projectModel.OrchardCollaborationProject = suiteCRMProjectPart.ContentItem;
                        projectModel.IsSync = true;
                        projectModel.LastSyncTime = suiteCRMProjectPart.LastSyncTime;
                        projectModel.SuiteCRMProject = this.Convert(suiteCRMProject);
                        projectModel.OrchardCollaborationTitle = orchardProject.Record.Title;
                        returnValue.Add(projectModel);
                    }

                    suiteCRMTransaction.Commit();
                }
                catch (Exception ex)
                {
                    suiteCRMTransaction.Rollback();
                    throw ex;
                }
            }

            return returnValue;
        }

        public IEnumerable<SuiteCRMTaskDetailViewModel> CopyOrchardTicketsToSuite(CopyOrchardTasksToSuiteViewModel model)
        {
            List<SuiteCRMTaskDetailViewModel> returnValue = new List<SuiteCRMTaskDetailViewModel>();

            using (var connection = Helper.GetConnection(this.services, this.Logger))
            using (SuiteCRMTaskUnitOfWork taskRepository = new SuiteCRMTaskUnitOfWork(connection))
            using (SuiteCRMEmailAddressBeanUnitOfWork suiteCRMEmailAddressBeanUnitOfWork = new SuiteCRMEmailAddressBeanUnitOfWork(taskRepository))
            using (SuiteCRMEmailAddressUnitOfWork suiteCRMEmailAddressUnitOfWork = new SuiteCRMEmailAddressUnitOfWork(taskRepository))
            using (SuiteCRMProjectTaskUnitOfWork projectTasksRepository = new SuiteCRMProjectTaskUnitOfWork(taskRepository))
            using (var suiteCRMTransaction = taskRepository.BeginTransaction())
            {
                TicketContext context = new TicketContext();
                context.ProjectTaskUnitOfWork = projectTasksRepository;
                context.Priorities = this.priorityRepository.Table.ToList();
                context.StatusList = this.statusRepository.Table.ToList();

                try
                {
                    var taskIds = model.Tasks.Where(c => !string.IsNullOrEmpty(c.SuiteCRMId)).Select(c => c.SuiteCRMId).ToArray();
                    var suiteTasks = taskRepository.GetTasks(taskIds);
                    var suiteProjectTasks = projectTasksRepository.GetTasks(taskIds);
                    var orchardTickets = this.services
                                              .ContentManager
                                              .GetMany<SuiteCRMTaskPart>(
                                                model.Tasks.Where(c => c.OrchardCollaborationTicketId.HasValue).Select(c => c.OrchardCollaborationTicketId.Value),
                                                VersionOptions.Published,
                                                new QueryHints().ExpandParts<TicketPart>());

                    foreach (var item in model.Tasks)
                    {
                        if (item.OrchardCollaborationTicketId == null)
                        {
                            continue;
                        }

                        var ticket = orchardTickets.FirstOrDefault(c => c.Id == item.OrchardCollaborationTicketId.Value);

                        var suiteCRMTaskPart = ticket.As<SuiteCRMTaskPart>();
                        TicketPart ticketPart = ticket.As<TicketPart>();
                        ContentItemPermissionPart permissionPart = ticket.As<ContentItemPermissionPart>();
                        AttachToProjectPart attachToProjectPart = ticket.As<AttachToProjectPart>();
                        SuiteCRMProjectPart projectPart = null;

                        if (!this.IsSyncingTicketValid(item, ticket, out projectPart))
                        {
                            continue;
                        }

                        project_task suiteCRMProjectTask = null;
                        task suiteCRMTask = null;

                        if (!string.IsNullOrEmpty(suiteCRMTaskPart.ExternalId) && item.IsProjectTask)
                        {
                            suiteCRMProjectTask = suiteProjectTasks.FirstOrDefault(c => c.id == suiteCRMTaskPart.ExternalId);
                        }

                        if (!string.IsNullOrEmpty(suiteCRMTaskPart.ExternalId) && !item.IsProjectTask)
                        {
                            suiteCRMTask = suiteTasks.FirstOrDefault(c => c.id == suiteCRMTaskPart.ExternalId);
                        }

                        if (suiteCRMProjectTask == null && item.IsProjectTask)
                        {
                            suiteCRMProjectTask = new project_task();
                            suiteCRMProjectTask.project_id = item.SuiteCRMId;
                            projectTasksRepository.Add(suiteCRMProjectTask);
                        }

                        if (suiteCRMTask == null && !item.IsProjectTask)
                        {
                            suiteCRMTask = new task();
                            taskRepository.Add(suiteCRMTask);
                        }

                        CommonPart commonPart = ticketPart.As<CommonPart>();
                        DateTime? lastOrchardTicketChangeDate = commonPart.ModifiedUtc ?? commonPart.CreatedUtc;
                        if (suiteCRMProjectTask != null &&
                            (
                                !item.DoNotOverrideNewerValues ||
                                suiteCRMProjectTask.date_modified == null ||
                                (lastOrchardTicketChangeDate.HasValue &&
                                suiteCRMProjectTask.date_modified.Value <= lastOrchardTicketChangeDate.Value)
                            ))
                        {
                            this.Copy(ticketPart, permissionPart, suiteCRMProjectTask, context);
                            suiteCRMProjectTask.project_id = projectPart.ExternalId;
                            projectTasksRepository.Save();
                        }

                        if (suiteCRMTask != null &&
                            (
                                !item.DoNotOverrideNewerValues ||
                                suiteCRMTask.date_modified == null ||
                                (lastOrchardTicketChangeDate.HasValue &&
                                suiteCRMTask.date_modified.Value <= lastOrchardTicketChangeDate.Value)
                            ))
                        {
                            this.Copy(ticketPart, permissionPart, suiteCRMTask, context);

                            // set  contact
                            if (string.IsNullOrEmpty(suiteCRMTask.created_by) && commonPart.Owner != null)
                            {
                                var emailAddress = suiteCRMEmailAddressUnitOfWork.GetByEmail(commonPart.Owner.Email);
                                if (emailAddress != null){
                                    var contact = suiteCRMEmailAddressBeanUnitOfWork.GetBeanIdOfEmailAddress(Helper.ContactsModuleName, new[] { emailAddress.id }).FirstOrDefault();

                                    if (contact != null)
                                    {
                                        suiteCRMTask.parent_id = contact.id;
                                        suiteCRMTask.parent_type = Helper.ContactsModuleName;
                                    }
                                }
                            }

                            projectTasksRepository.Save();
                        }

                        suiteCRMTaskPart.ExternalId = item.IsProjectTask ? suiteCRMProjectTask.id : suiteCRMTask.id;
                        suiteCRMTaskPart.LastSyncTime = DateTime.UtcNow;
                        suiteCRMTaskPart.TaskType = item.IsProjectTask ? SuiteCRMTaskPart.SuiteCRMProjectTaskTypeValue : SuiteCRMTaskPart.SuiteCRMTaskTypeValue;
                        this.services.ContentManager.Publish(ticket.ContentItem);

                        SuiteCRMTaskDetailViewModel returnValueItem = new SuiteCRMTaskDetailViewModel();
                        returnValueItem.OrchardCollaborationTicket = ticket.ContentItem;
                        returnValueItem.IsSync = true;
                        returnValueItem.IsProjectTask = item.IsProjectTask;
                        returnValueItem.LastSyncTime = suiteCRMTaskPart.LastSyncTime;
                        returnValueItem.SuiteCRMTaskId = suiteCRMTaskPart.ExternalId;
                        returnValue.Add(returnValueItem);
                    }

                    suiteCRMTransaction.Commit();
                }
                catch (Exception ex)
                {
                    suiteCRMTransaction.Rollback();
                    throw ex;
                }
            }

            return returnValue;
        }

        private bool IsSyncingTicketValid(CopyOrchardTasksToSuiteViewModel.TaskIdentifiers item, SuiteCRMTaskPart ticket, out SuiteCRMProjectPart projectPart)
        {
            projectPart = null;

            if (ticket == null)
            {
                return false;
            }

            var suiteCRMTaskPart = ticket.As<SuiteCRMTaskPart>();

            if (suiteCRMTaskPart == null)
            {
                return false;
            }

            TicketPart ticketPart = ticket.As<TicketPart>();

            if (ticketPart == null)
            {
                return false;
            }

            if (!item.SyncSubTasks && ticketPart.Record.Parent != null)
            {
                return false;
            }

            AttachToProjectPart attachToProjectPart = ticket.As<AttachToProjectPart>();
            if (item.IsProjectTask && (attachToProjectPart == null || attachToProjectPart.Record.Project == null))
            {
                return false;
            }

            if (item.IsProjectTask)
            {
                var project = this.services.ContentManager.Get(attachToProjectPart.Record.Project.Id);

                if (project == null)
                {
                    return false;
                }

                projectPart = project.As<SuiteCRMProjectPart>();

                if (string.IsNullOrEmpty(projectPart.ExternalId))
                {
                    return false;
                }
            }

            return true;
        }

        private void CopyOrchardTicketsToSuiteCRM(TicketContext context, CopyOrchardProjectToSuiteViewModel.ProjectIdentifiers syncSettings)
        {
            if (syncSettings.OrchardCollaborationProjectId == null)
            {
                throw new ArgumentNullException("OrchardCollaborationProjectId must not be null");
            }

            if (string.IsNullOrEmpty(syncSettings.SuiteCRMId))
            {
                throw new ArgumentNullException("SuiteCRMId must not be null");
            }

            var orchardTickets = this.searchTicketService.SearchByDatabase(new PagerParametersWithSortFields() { PageSize = 0 }, new PostedTicketSearchViewModel { ProjectId = syncSettings.OrchardCollaborationProjectId });
            List<project_task> suiteCRMTasks = new List<project_task>();

            if (!string.IsNullOrEmpty(syncSettings.SuiteCRMId))
            {
                suiteCRMTasks.AddRange(context.ProjectTaskUnitOfWork.GetProjectTasks(syncSettings.SuiteCRMId));
            }

            foreach (var ticket in orchardTickets)
            {
                var suiteCRMTaskPart = ticket.As<SuiteCRMTaskPart>();

                if (suiteCRMTaskPart == null)
                {
                    continue;
                }

                TicketPart ticketPart = ticket.As<TicketPart>();
                ContentItemPermissionPart permissionPart = ticket.As<ContentItemPermissionPart>();

                if (ticketPart == null)
                {
                    continue;
                }

                if (!syncSettings.SyncSubTasks && ticketPart.Record.Parent != null)
                {
                    continue;
                }

                project_task suiteCRMProjectTask = null;

                if (!string.IsNullOrEmpty(suiteCRMTaskPart.ExternalId))
                {
                    suiteCRMProjectTask = suiteCRMTasks.FirstOrDefault(c => c.id == suiteCRMTaskPart.ExternalId);
                }

                if (suiteCRMProjectTask == null)
                {
                    suiteCRMProjectTask = new project_task();
                    suiteCRMProjectTask.project_id = syncSettings.SuiteCRMId;
                    context.ProjectTaskUnitOfWork.Add(suiteCRMProjectTask);
                }

                CommonPart commonPart = ticketPart.As<CommonPart>();
                DateTime? lastOrchardTicketChangeDate = commonPart.ModifiedUtc ?? commonPart.CreatedUtc;
                if (!syncSettings.DoNotOverrideNewerValues ||
                    suiteCRMProjectTask.date_modified == null ||
                    (lastOrchardTicketChangeDate.HasValue &&
                    suiteCRMProjectTask.date_modified.Value <= lastOrchardTicketChangeDate.Value))
                {
                    this.Copy(ticketPart, permissionPart, suiteCRMProjectTask, context);
                    context.ProjectTaskUnitOfWork.Save();
                }

                suiteCRMTaskPart.ExternalId = suiteCRMProjectTask.id;
                suiteCRMTaskPart.LastSyncTime = DateTime.UtcNow;
                suiteCRMTaskPart.TaskType = SuiteCRMTaskPart.SuiteCRMProjectTaskTypeValue;
                this.services.ContentManager.Publish(ticket.ContentItem);
            }
        }

        private void CopySuiteCRMTasksToOrchardTickets(TicketContext context, CopyOrchardProjectToSuiteViewModel.ProjectIdentifiers syncSettings)
        {
            if (syncSettings.OrchardCollaborationProjectId == null)
            {
                throw new ArgumentNullException("OrchardCollaborationProjectId must not be null");
            }

            if (string.IsNullOrEmpty(syncSettings.SuiteCRMId))
            {
                throw new ArgumentNullException("SuiteCRMId must not be null");
            }

            List<project_task> suiteCRMTasks = context.ProjectTaskUnitOfWork.GetProjectTasks(syncSettings.SuiteCRMId).ToList();
            var orchardTickets = this.searchTicketService.SearchByDatabase(
                new PagerParametersWithSortFields() { PageSize = 0 },
                new PostedTicketSearchViewModel { ProjectId = syncSettings.OrchardCollaborationProjectId })
                .Select(c => c.As<SuiteCRMTaskPart>())
                .Where(c => c != null)
                .ToList();

            foreach (var suiteCRMTask in suiteCRMTasks)
            {
                var orchardTicket = orchardTickets.FirstOrDefault(c => c.ExternalId == suiteCRMTask.id);
                dynamic ticketSnapshot = null;

                ContentItem ticketContentItem = null;
                bool isNew = false;
                if (orchardTicket == null)
                {
                    isNew = true;
                    ticketContentItem = this.services.ContentManager.Create("Ticket");
                }
                else
                {
                    ticketContentItem = orchardTicket.ContentItem;
                    ticketSnapshot = this.streamService.TakeSnapshot(ticketContentItem);
                }

                TicketPart ticketPart = ticketContentItem.As<TicketPart>();
                SuiteCRMTaskPart taskPart = ticketContentItem.As<SuiteCRMTaskPart>();
                AttachToProjectPart attachToProjectPart = ticketContentItem.As<AttachToProjectPart>();

                // the values will be overridde in case user doesn't care about update time (item.DoNotOverrideNewerValues == false) or
                // the target modified date is less than source modified date
                DateTime? lastSuiteCRMChangeDate = suiteCRMTask.date_modified ?? suiteCRMTask.date_entered;
                CommonPart commonPart = ticketPart.As<CommonPart>();
                if (!syncSettings.DoNotOverrideNewerValues ||
                    isNew ||
                    (lastSuiteCRMChangeDate.HasValue && commonPart.ModifiedUtc <= lastSuiteCRMChangeDate.Value))
                {
                    if (attachToProjectPart != null)
                    {
                        attachToProjectPart.Record.Project = new ProjectPartRecord { Id = syncSettings.OrchardCollaborationProjectId.Value };
                    }

                    this.Copy(suiteCRMTask, ticketPart, context);
                    this.services.ContentManager.Publish(ticketContentItem);
                    this.streamService.WriteChangesToStreamActivity(ticketContentItem, ticketSnapshot, null);
                }

                taskPart.ExternalId = suiteCRMTask.id;
                taskPart.LastSyncTime = DateTime.UtcNow;
                taskPart.TaskType = SuiteCRMTaskPart.SuiteCRMProjectTaskTypeValue;
            }
        }

        private void Copy(project_task source, TicketPart destination, TicketContext context)
        {
            destination.Record.Title = source.name;
            destination.Record.Description = source.description;

            // priority
            if (!string.IsNullOrEmpty(source.priority))
            {
                var priority = context.Priorities.FirstOrDefault(c => c.Name.ToLower().Trim() == source.priority.ToLower().Trim());
                if (priority != null)
                {
                    destination.PriorityRecord = new PriorityRecord { Id = priority.Id };
                }
            }

            // status
            if (!string.IsNullOrEmpty(source.status))
            {
                var status = context.StatusList.FirstOrDefault(c => c.Name.ToLower().Trim() == source.status.ToLower().Trim());
                if (status != null)
                {
                    destination.StatusRecord = new StatusRecord { Id = status.Id };
                }
            }
        }

        private void Copy(TicketPart source, ContentItemPermissionPart sourcePermissionPart, project_task destination, TicketContext context)
        {
            destination.name = source.Record.Title.Length > 50 ? source.Record.Title.Substring(0, 50) : source.Record.Title;
            destination.task_number = source.Record.Identity != null ? (int?)source.Record.Identity.Id : null;

            // status
            if (source.Record.StatusRecord != null)
            {
                var status = context.StatusList.FirstOrDefault(c => c.Id == source.Record.StatusRecord.Id);
                if (status != null)
                {
                    destination.status = status.Name;
                }
            }

            // priority
            if (source.Record.PriorityRecord != null)
            {
                var priority = context.Priorities.FirstOrDefault(c => c.Id == source.Record.PriorityRecord.Id);
                if (priority != null)
                {
                    destination.priority = priority.Name;
                }
            }

            CommonPart commonPart = source.As<CommonPart>();
            if (commonPart != null && commonPart.ModifiedUtc.HasValue)
            {
                destination.date_modified = commonPart.ModifiedUtc;
            }
   
            // assignee user
            destination.assigned_user_id = this.GetAssigneeUserExternalId(sourcePermissionPart);
            destination.created_by = this.GetOwnerUserExternalId(commonPart);
        }

        private string GetAssigneeUserExternalId(ContentItemPermissionPart sourcePermissionPart)
        {
            // assignee user
            var assigneeUser = sourcePermissionPart.Record.Items.FirstOrDefault(c => c.AccessType == ContentItemPermissionAccessTypes.Assignee && c.User != null);
            if (assigneeUser != null)
            {
                var user = this.services.ContentManager.Get<IUser>(assigneeUser.User.Id);
                if (user != null)
                {
                    SuiteCRMUserPart userPart = user.As<SuiteCRMUserPart>();

                    if (userPart != null && !string.IsNullOrEmpty(userPart.ExternalId))
                        return userPart.ExternalId;
                }
            }

            return null;
        }

        private string GetOwnerUserExternalId(CommonPart commonPart)
        {
            // assignee user
            if (commonPart.Owner != null)
            {
                var user = this.services.ContentManager.Get<IUser>(commonPart.Owner.Id);
                if (user != null)
                {
                    SuiteCRMUserPart userPart = user.As<SuiteCRMUserPart>();

                    if (userPart != null && !string.IsNullOrEmpty(userPart.ExternalId))
                    {
                        return userPart.ExternalId;
                    }
                }
            }

            return null;
        }

        private void Copy(TicketPart source, ContentItemPermissionPart sourcePermissionPart, task destination, TicketContext context)
        {
            destination.name = source.Record.Title.Length > 50 ? source.Record.Title.Substring(0, 50) : source.Record.Title;
            destination.date_due = source.Record.DueDate;

            // status
            if (source.Record.StatusRecord != null)
            {
                var status = context.StatusList.FirstOrDefault(c => c.Id == source.Record.StatusRecord.Id);
                if (status != null)
                {
                    destination.status = status.Name;
                }
            }

            // priority
            if (source.Record.PriorityRecord != null)
            {
                var priority = context.Priorities.FirstOrDefault(c => c.Id == source.Record.PriorityRecord.Id);
                if (priority != null)
                {
                    destination.priority = priority.Name;
                }
            }

            CommonPart commonPart = source.As<CommonPart>();
            if (commonPart != null && commonPart.ModifiedUtc.HasValue)
            {
                destination.date_modified = commonPart.ModifiedUtc;
            }

            destination.assigned_user_id = this.GetAssigneeUserExternalId(sourcePermissionPart);
            destination.created_by = this.GetOwnerUserExternalId(commonPart);
        }

        public SuiteCRMViewModel Convert(project project)
        {
            return new SuiteCRMViewModel
            {
                Id = project.id,
                Name = project.name,
                Description = project.description,
                CreationDateTime = project.date_entered.HasValue ? (DateTime?)CRMHelper.SetSiteTimeZone(this.services, project.date_entered.Value) : null,
                ModifiedDateTime = project.date_modified.HasValue ? (DateTime?)CRMHelper.SetSiteTimeZone(this.services, project.date_modified.Value) : null,
            };
        }

        public void Copy(project source, ProjectPart destination)
        {
            destination.Title = source.name;
            destination.Description = source.description;
        }

        public void Copy(ProjectPart source, project destination)
        {
            destination.name = source.Record.Title.Length > 50 ? source.Record.Title.Substring(0, 50) : source.Record.Title;
            destination.description = source.Record.Description;

            CommonPart commonPart = source.As<CommonPart>();
            if (commonPart != null && commonPart.ModifiedUtc.HasValue)
            {
                destination.date_modified = commonPart.ModifiedUtc;
            }
        }
        class TicketContext
        {
            public SuiteCRMProjectTaskUnitOfWork ProjectTaskUnitOfWork { get; set; }
            public List<PriorityRecord> Priorities { get; set; }
            public List<StatusRecord> StatusList { get; set; }
        }
    }
}