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

using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Activities;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Providers.Filters;
using Orchard.Data;
using Orchard.Projections.Models;
using Orchard.Reporting.Models;
using Orchard.Workflows.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Commands
{
    public class ProjectCommand : DefaultOrchardCommandHandler
    {
        protected readonly IContentManager contentManager;
        protected readonly IRepository<LayoutRecord> layoutRepository;
        protected readonly IRepository<FilterRecord> filterRepository;
        protected readonly IRepository<FilterGroupRecord> filterGroupRepository;
        private readonly IRepository<ReportRecord> reportRepository;
        private readonly IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository;
        private readonly IRepository<ActivityRecord> activityRepository;
        private readonly IRepository<TransitionRecord> transitionRepository;
        private readonly IRepository<SortCriterionRecord> sortRepository;
        private readonly IRepository<EmailTemplateRecord> emailTemplatesRepository;
        private readonly IBasicDataService basicDataService;

        public ProjectCommand(
            IRepository<SortCriterionRecord> sortRepository,
            IRepository<EmailTemplateRecord> emailTemplatesRepository,
            IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository,
            IRepository<ActivityRecord> activityRepository,
            IRepository<TransitionRecord> transitionRepository,
            IRepository<ReportRecord> reportRepository,
            IRepository<LayoutRecord> layoutRepository,
            IContentManager contentManager,
            IRepository<FilterRecord> filterRepository,
            IRepository<FilterGroupRecord> filterGroupRepository,
            IBasicDataService basicDataService
            )
        {
            this.basicDataService = basicDataService;
            this.sortRepository = sortRepository;
            this.emailTemplatesRepository = emailTemplatesRepository;
            this.layoutRepository = layoutRepository;
            this.filterGroupRepository = filterGroupRepository;
            this.filterRepository = filterRepository;
            this.contentManager = contentManager;
            this.reportRepository = reportRepository;
            this.transitionRepository = transitionRepository;
            this.activityRepository = activityRepository;
            this.workflowDefinitionRepository = workflowDefinitionRepository;
        }

        [OrchardSwitch]
        public string SendEmailToFollowersTicket
        {
            get
            {
                return this.sendEmailToFollowersTicket;
            }
            set
            {
                this.sendEmailToFollowersTicket = value;
            }
        }
        private string sendEmailToFollowersTicket = "Send Email to followers";

        [CommandName("CreateSendEmailToFollowersWorkflow")]
        public void CreateSendEmailToFollowersWorkflow()
        {
            WorkflowDefinitionRecord workflow = new WorkflowDefinitionRecord { Name = this.SendEmailToFollowersTicket, Enabled = true };
            this.workflowDefinitionRepository.Create(workflow);
            var emailTemplate = this.emailTemplatesRepository.Table.FirstOrDefault(c => c.TypeId == (int)EmailTemplateType.FollowersNotification);

            if (emailTemplate == null)
            {
                throw new NullReferenceException();
            }

            ActivityRecord itemPublishRecord = new ActivityRecord
            {
                WorkflowDefinitionRecord = workflow,
                Start = true,
                X = 552,
                Y = 227,
                State = "{}",
                Name = NewActivityStreamActivity.ActivityStreamActivityName
            };
            this.activityRepository.Create(itemPublishRecord);

            ActivityRecord timerActivity = new ActivityRecord
            {
                WorkflowDefinitionRecord = workflow,
                Start = false,
                X = 552,
                Y = 388,
                State = "{\"Amount\":\"1\",\"Unity\":\"Minute\"}",
                Name = OrchardCollaborationTimerActivity.ActionName
            };
            this.activityRepository.Create(timerActivity);

            ActivityRecord sendEmailActivityRecord = new ActivityRecord
            {
                State = string.Format(
                   CultureInfo.InvariantCulture,
                "{{\"{0}\":\"{1}\"}}",
                   EmailTemplateActivityForm.EmailTemplateIdFieldName,
                   emailTemplate.Id.ToString(CultureInfo.InvariantCulture)),
                Name = "SendEmailToFollowersActivity",
                WorkflowDefinitionRecord = workflow,
                X = 494,
                Y = 588,
                Start = false
            };

            this.activityRepository.Create(sendEmailActivityRecord);

            TransitionRecord timerTransitionRecord = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = timerActivity,
                SourceActivityRecord = itemPublishRecord,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(timerTransitionRecord);

            TransitionRecord sendEmailTransitionRecord = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = sendEmailActivityRecord,
                SourceActivityRecord = timerActivity,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(sendEmailTransitionRecord);

            this.workflowDefinitionRepository.Flush();
            this.activityRepository.Flush();
            this.transitionRepository.Flush();
        }

        [CommandName("CreateProjectActivityStreamQuery")]
        public QueryPart CreateProjectActivityStreamQuery()
        {
            var query = this.GetQuery(QueryNames.ProjectActivityStreamQueryName);

            if (query != null)
            {
                return query.As<QueryPart>();
            }

            query = this.contentManager.Create("Query");
            query.As<TitlePart>().Title = QueryNames.ProjectActivityStreamQueryName;
            this.contentManager.Publish(query);
            QueryPart queryPart = query.As<QueryPart>();
            var attachToProjectFilterGroup = queryPart.Record.FilterGroups.FirstOrDefault();

            // AttachToProject
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><Project_Id>{0}</Project_Id></Form>", "{AttachToProject.ProjectId}");
            FilterRecord attachToProjectFilterRecord = this.CreateFilter(AttachToProjectFilter.CategoryName, AttachToProjectFilter.IdFilterType, state, attachToProjectFilterGroup);
            this.filterRepository.Flush();

            var projectFilterGroup = new FilterGroupRecord { QueryPartRecord = queryPart.Record };
            queryPart.Record.FilterGroups.Add(projectFilterGroup);
            this.filterGroupRepository.Create(projectFilterGroup);

            // project ContentType filter
            this.CreateContentTypeFilter(ContentTypes.ProjectContentType, projectFilterGroup);

            state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><Project_Id>{0}</Project_Id></Form>", "{Project.ProjectId}");
            this.CreateFilter(ProjectFilter.CategoryName, ProjectFilter.IdFilterType, state, projectFilterGroup);
            this.filterGroupRepository.Flush();
            this.filterRepository.Flush();

            return queryPart;
        }

        [CommandName("CreateWikiActivityStreamQuery")]
        public QueryPart CreateWikiActivityStreamQuery()
        {
            var query = this.GetQuery(QueryNames.WikiActivityStreamQueryName);

            if (query != null)
            {
                return query.As<QueryPart>();
            }

            query = this.contentManager.Create("Query");
            query.As<TitlePart>().Title = QueryNames.WikiActivityStreamQueryName;
            this.contentManager.Publish(query);
            QueryPart queryPart = query.As<QueryPart>();
            var filterGroup = queryPart.Record.FilterGroups.FirstOrDefault();

            // AttachToProject
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><Project_Id>{0}</Project_Id></Form>", "{AttachToProject.ProjectId}");
            FilterRecord attachToProjectFilterRecord = this.CreateFilter(AttachToProjectFilter.CategoryName, AttachToProjectFilter.IdFilterType, state, filterGroup);
            this.filterRepository.Flush();

            // ContentType filter
            this.CreateContentTypeFilter(string.Join(",", new[] { ContentTypes.WikiItemType, ContentTypes.FolderContentType }), filterGroup);

            this.filterRepository.Flush();

            return queryPart;
        }

        [CommandName("CreateProjectTicketsQuery")]
        public QueryPart CreateProjectTicketsQuery()
        {
            return this.CreateProjectAttachableItemsQuery(QueryNames.ProjectTicketsQueryName, ContentTypes.TicketContentType, "ProjectTickets");
        }

        [CommandName("CreateNotClosedProjectTicketsQuery")]
        public QueryPart CreateNotClosedProjectTicketsQuery()
        {
            var query = this.CreateProjectAttachableItemsQuery(QueryNames.ProjectNoneClosedTicketsQueryName, ContentTypes.TicketContentType, "ProjectTickets");

            var filterGroup = query.Record.FilterGroups.FirstOrDefault();

            // not closed
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><NotEqual>true</NotEqual><StatusType_Id>{0}</StatusType_Id></Form>", StatusRecord.ClosedStatus.ToString(CultureInfo.InvariantCulture));
            this.CreateFilter(TicketFieldsFilter.CategoryName, TicketFieldsFilter.StatusTypeFilter, state, filterGroup);
            this.filterRepository.Flush();

            return query;
        }

        [CommandName("CreateHighPriorityProjectTicketsQuery")]
        public QueryPart CreateHighPriorityProjectTicketsQuery()
        {
            var priorities = this.basicDataService.GetPriorities();
            if (priorities == null || priorities.Count() == 0)
            {
                return null;
            }

            var query = this.CreateProjectAttachableItemsQuery(QueryNames.ProjectCriticalNoneClosedTicketsQueryName, ContentTypes.TicketContentType, QueryNames.ProjectTicketsSummaryShapeName);

            var filterGroup = query.Record.FilterGroups.FirstOrDefault();

            // not closed
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><NotEqual>true</NotEqual><StatusType_Id>{0}</StatusType_Id></Form>", StatusRecord.ClosedStatus.ToString(CultureInfo.InvariantCulture));
            this.CreateFilter(TicketFieldsFilter.CategoryName, TicketFieldsFilter.StatusTypeFilter, state, filterGroup);
            this.filterRepository.Flush();

            var highestPriorityOrder = priorities.OrderByDescending(c => c.OrderId).First().Id;

            // Highest priority
            state = string.Format(CultureInfo.InvariantCulture, "<Form><Priority_Id>{0}</Priority_Id></Form>", highestPriorityOrder.ToString(CultureInfo.InvariantCulture));
            this.CreateFilter(TicketFieldsFilter.CategoryName, TicketFieldsFilter.PriorityFilter, state, filterGroup);
            this.filterRepository.Flush();

            return query;
        }

        [CommandName("CreateOverdueProjectTicketsQuery")]
        public QueryPart CreateOverdueProjectTicketsQuery()
        {
            var query = this.CreateProjectAttachableItemsQuery(QueryNames.ProjectOverdueTicketsQueryName, ContentTypes.TicketContentType, QueryNames.ProjectTicketsSummaryShapeName);
            var filterGroup = query.Record.FilterGroups.FirstOrDefault();

            // not closed
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><NotEqual>true</NotEqual><StatusType_Id>{0}</StatusType_Id></Form>", StatusRecord.ClosedStatus.ToString(CultureInfo.InvariantCulture));
            this.CreateFilter(TicketFieldsFilter.CategoryName, TicketFieldsFilter.StatusTypeFilter, state, filterGroup);
            this.filterRepository.Flush();

            // Max Due Date
            state = string.Format(CultureInfo.InvariantCulture, "<Form><MaxDueDate>{0}</MaxDueDate></Form>", DateTime.UtcNow.Date.ToString());
            this.CreateFilter(TicketFieldsFilter.CategoryName, TicketFieldsFilter.TicketDueDateType, state, filterGroup);
            this.filterRepository.Flush();
            return query;
        }

        [CommandName("CreateProjectUnassignedTicketsQuery")]
        public QueryPart CreateProjectUnassignedTicketsQuery()
        {
            var query = this.CreateProjectAttachableItemsQuery(QueryNames.ProjectUnassignedTicketsQueryName, ContentTypes.TicketContentType, QueryNames.ProjectTicketsSummaryShapeName);

            var filterGroup = query.Record.FilterGroups.FirstOrDefault();

            // unassigned items
            this.CreateFilter(ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.UnassignedItems, string.Empty, filterGroup);
            this.filterRepository.Flush();

            return query;
        }

        [CommandName("CreateProjectDiscussionsQuery")]
        public QueryPart CreateProjectDiscussionQuery()
        {
            return this.CreateProjectAttachableItemsQuery(QueryNames.ProjectDiscussionsQueryName, ContentTypes.DiscussionContentType, QueryNames.ProjectProjectionShapeName);
        }

        [CommandName("CreateProjectMilestonesQuery")]
        public QueryPart CreateProjectMilestonesQuery()
        {
            var query = this.CreateProjectAttachableItemsQuery(QueryNames.ProjectMilestonesQueryName, ContentTypes.MilestoneContentType, QueryNames.ProjectProjectionShapeName, "Shape", false);
            var sortRecord = this.CreateSortRecord("Milestone", MilestonePart.StartTimeFieldName, "<Form><Sort>false</Sort></Form>", query.Record);
            this.sortRepository.Flush();

            return query;
        }

        [CommandName("CreateProjectDiscussionsSummaryQuery")]
        public QueryPart CreateProjectDiscussionSummaryQuery()
        {
            return this.CreateProjectAttachableItemsQuery(QueryNames.ProjectDiscussionsSummaryQueryName, ContentTypes.DiscussionContentType, QueryNames.ProjectSummaryProjectionPortletShapeName, QueryNames.ProjectDashboardListLayout);
        }

        [CommandName("CreateProjectWikiesSummaryQuery")]
        public QueryPart CreateProjectWikiesSummaryQuery()
        {
            return this.CreateProjectAttachableItemsQuery(QueryNames.ProjectWikiesSummaryQueryName, ContentTypes.WikiItemType, QueryNames.ProjectSummaryProjectionPortletShapeName, QueryNames.ProjectDashboardListLayout);
        }

        [CommandName("CreateProjectTicketsGroupByStatusReport")]
        public void CreateProjectTicketsGroupByStatusReport()
        {
            this.CreateDataReport(
                QueryNames.ProjectTicketsQueryName,
                QueryNames.ProjectTicketGroupByStatusDataReportName,
                "Tickets summary group by status in List",
                ChartTypes.SimpleList,
                TicketGroupByParameterProvider.CategoryName,
                TicketGroupByParameterProvider.GroupingByTicketStatus,
                AggregateMethods.Count);

            this.CreateDataReport(
                QueryNames.ProjectTicketsQueryName,
                QueryNames.ProjectTicketGroupByStatusChartDataReportName,
                "Tickets summary group by status in Chart",
                ChartTypes.PieChart,
                TicketGroupByParameterProvider.CategoryName,
                TicketGroupByParameterProvider.GroupingByTicketStatus,
                AggregateMethods.Count);
        }

        [CommandName("CreateProjectOverdueTicketsGroupByTicketTypeReport")]
        public void CreateProjectOverdueTicketsGroupByTicketTypeReport()
        {
            this.CreateDataReport(
                QueryNames.ProjectOverdueTicketsQueryName,
                QueryNames.ProjectOverdueTicketGroupByTicketTypeDataReportName,
                "Over due tickets group by Ticket Type",
                ChartTypes.SimpleList,
                TicketGroupByParameterProvider.CategoryName,
                TicketGroupByParameterProvider.GroupingByTicketType,
                AggregateMethods.Count);

            this.CreateDataReport(
                QueryNames.ProjectOverdueTicketsQueryName,
                QueryNames.ProjectOverdueTicketGroupByTicketTypeChartDataReportName,
                "Over due tickets group by Ticket Type in Chart",
                ChartTypes.PieChart,
                TicketGroupByParameterProvider.CategoryName,
                TicketGroupByParameterProvider.GroupingByTicketType,
                AggregateMethods.Count);
        }

        [CommandName("CreateProjectNotClosedTicketsGroupByPriorityReport")]
        public void CreateProjectNotTicketsGroupByPriorityReport()
        {
            this.CreateDataReport(
                QueryNames.ProjectNoneClosedTicketsQueryName,
                QueryNames.ProjectNotClosedTicketsGroupByPriorityDataReportName,
                "Active Tickets Grouped by Priority",
                ChartTypes.PieChart,
                TicketGroupByParameterProvider.CategoryName,
                TicketGroupByParameterProvider.GroupingByTicketPriority,
                AggregateMethods.Count);

            this.CreateDataReport(
                QueryNames.ProjectNoneClosedTicketsQueryName,
                QueryNames.ProjectNotClosedTicketsGroupByPriorityDataReportName,
                "Active Tickets Grouped by Priority (in list-view)",
                ChartTypes.SimpleList,
                TicketGroupByParameterProvider.CategoryName,
                TicketGroupByParameterProvider.GroupingByTicketPriority,
                AggregateMethods.Count);
        }

        protected void CreateDataReport(string queryName, string name, string title, ChartTypes chartType, string groupByCategory, string groupByType, AggregateMethods aggregateMethod)
        {
            var query = this.GetQuery(queryName);

            if (query == null)
            {
                return;
            }

            var queryPart = query.As<QueryPart>();

            ReportRecord newReport = new ReportRecord
            {
                Name = name,
                Title = title,
                Query = queryPart.Record,
                ChartType = (int)chartType,
                GroupByCategory = groupByCategory,
                GroupByType = groupByType,
                AggregateMethod = (int)aggregateMethod
            };

            this.reportRepository.Create(newReport);
            this.reportRepository.Flush();
        }

        protected FilterRecord CreateContentTypeFilter(string contnetType, FilterGroupRecord filterGroup)
        {
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><ContentTypes>{0}</ContentTypes></Form>", contnetType);
            return this.CreateFilter("Content", "ContentTypes", state, filterGroup);
        }

        protected ContentItem GetQuery(string title)
        {
            var query = this
                  .contentManager
                  .HqlQuery()
                  .ForType("Query")
                  .Where(c => c.ContentPartRecord<TitlePartRecord>(), c => c.Eq("Title", title))
                  .Slice(0, 1)
                  .FirstOrDefault();

            return query;
        }

        protected FilterRecord CreateFilter(string category, string type, string state, FilterGroupRecord filterGroup)
        {
            FilterRecord filterRecord = new FilterRecord();
            filterRecord.FilterGroupRecord = filterGroup;
            filterRecord.Category = category;
            filterRecord.Type = type;
            filterRecord.State = state;
            this.filterRepository.Create(filterRecord);

            return filterRecord;
        }

        protected SortCriterionRecord CreateSortRecord(string category, string type, string state, QueryPartRecord queryPartRecord)
        {
            SortCriterionRecord sortRecord = new SortCriterionRecord();
            sortRecord.Category = category;
            sortRecord.Type = type;
            sortRecord.State = state;
            sortRecord.QueryPartRecord = queryPartRecord;
            queryPartRecord.SortCriteria.Add(sortRecord);
            sortRecord.Position = 0;
            this.sortRepository.Create(sortRecord);

            return sortRecord;
        }

        private QueryPart CreateProjectAttachableItemsQuery(string title, string contentType, string shapeName)
        {
            return this.CreateProjectAttachableItemsQuery(title, contentType, shapeName, "Shape");
        }

        private QueryPart CreateProjectAttachableItemsQuery(string title, string contentType, string shapeName, string layoutName)
        {
            return this.CreateProjectAttachableItemsQuery(title, contentType, shapeName, layoutName, true);
        }

        private QueryPart CreateProjectAttachableItemsQuery(string title, string contentType, string shapeName, string layoutName, bool createDefaultSortCriteria)
        {
            var query = this.GetQuery(title);

            if (query != null)
            {
                return query.As<QueryPart>();
            }

            query = this.contentManager.Create("Query");
            query.As<TitlePart>().Title = title;
            this.contentManager.Publish(query);
            QueryPart queryPart = query.As<QueryPart>();
            var filterGroup = queryPart.Record.FilterGroups.FirstOrDefault();

            // AttachToProject
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><Project_Id>{0}</Project_Id></Form>", "{AttachToProject.ProjectId}");
            this.CreateFilter(AttachToProjectFilter.CategoryName, AttachToProjectFilter.IdFilterType, state, filterGroup);
            this.filterRepository.Flush();

            // ticket ContentType filter
            this.CreateContentTypeFilter(contentType, filterGroup);

            // All items visible by current user
            this.CreateFilter(ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.CurrentUserPermissions, state, filterGroup);

            if (createDefaultSortCriteria)
            {
                this.CreateSortRecord("CommonPartRecord", "PublishedUtc", "<Form><Sort>false</Sort></Form>", queryPart.Record);
            }

            string layoutState = string.Format(
                CultureInfo.InvariantCulture,
                "<Form><Category>Html</Category><Type>{0}</Type><Display>0</Display><DisplayType>Summary</DisplayType><ShapeType>{1}</ShapeType></Form>",
                layoutName,
                shapeName);

            // create layout
            LayoutRecord layout = new LayoutRecord
            {
                Type = "Shape",
                Category = "Html",
                QueryPartRecord = queryPart.Record,
                DisplayType = "Summary",
                Display = 0,
                State = layoutState
            };

            this.layoutRepository.Create(layout);
            this.layoutRepository.Flush();
            this.filterRepository.Flush();

            return queryPart;
        }
    }
}