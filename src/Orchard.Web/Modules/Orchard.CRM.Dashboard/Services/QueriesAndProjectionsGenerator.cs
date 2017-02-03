using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Project;
using Orchard.Data;
using Orchard.Fields.Fields;
using Orchard.Projections.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Dashboard.Services
{
    public class QueriesAndProjectionsGenerator : IQueriesAndProjectionsGenerator
    {
        protected readonly IContentManager contentManager;
        protected readonly IRepository<LayoutRecord> layoutRepository;
        protected readonly IRepository<FilterRecord> filterRepository;
        protected readonly IRepository<FilterGroupRecord> filterGroupRepository;
        private readonly IRepository<SortCriterionRecord> sortRepository;

        public QueriesAndProjectionsGenerator(
            IRepository<SortCriterionRecord> sortRepository,
            IRepository<LayoutRecord> layoutRepository,
            IContentManager contentManager,
            IRepository<FilterRecord> filterRepository,
            IRepository<FilterGroupRecord> filterGroupRepository)
        {
            this.sortRepository = sortRepository;
            this.layoutRepository = layoutRepository;
            this.filterGroupRepository = filterGroupRepository;
            this.filterRepository = filterRepository;
            this.contentManager = contentManager;
        }

        public FilterRecord CreateContentTypeFilter(string contnetType, FilterGroupRecord filterGroup)
        {
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><ContentTypes>{0}</ContentTypes></Form>", contnetType);
            return this.CreateFilter("Content", "ContentTypes", state, filterGroup);
        }

        public QueryPart GetQuery(string title)
        {
            var query = this.contentManager
                .HqlQuery()
                .ForType("Query")
                .Where(c => c.ContentPartRecord<TitlePartRecord>(), c => c.Eq("Title", title))
                .Slice(0, 1)
                .FirstOrDefault();

            return query != null ? query.As<QueryPart>() : null;
        }

        public FilterRecord CreateFilter(string category, string type, string state, FilterGroupRecord filterGroup)
        {
            FilterRecord filterRecord = new FilterRecord();
            filterRecord.FilterGroupRecord = filterGroup;
            filterRecord.Category = category;
            filterRecord.Type = type;
            filterRecord.State = state;
            this.filterRepository.Create(filterRecord);

            return filterRecord;
        }

        public SortCriterionRecord CreateSortRecord(string category, string type, string state, QueryPartRecord queryPartRecord)
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

        public ContentItem CreateProjection(string contentType, string queryName, string title, string itemContentType)
        {
            return CreateProjection(contentType, queryName, title, itemContentType, 20);
        }

        public ContentItem CreateProjection(string contentType, string queryName, string title, string itemContentType, int maxItems)
        {
            // get query
            var query = this.GetQuery(queryName);
            if (query != null)
            {
                var contentItem = contentManager.New(contentType);
                contentManager.Create(contentItem);

                // projection
                var projection = contentItem.As<ProjectionWithDynamicSortPart>();
                projection.Record.QueryPartRecord = query.Record;
                projection.Record.MaxItems = maxItems;

                // layout
                var layout = this.layoutRepository.Table.FirstOrDefault(c => c.QueryPartRecord.Id == query.Id && c.Category == "Html" && c.Type == "Shape");
                if (layout != null)
                {
                    projection.Record.LayoutRecord = layout;
                }

                // Title
                TitlePart titlePart = contentItem.As<TitlePart>();
                titlePart.Title = title;

                // item type
                var projectProjectionPart = contentItem.Parts.FirstOrDefault(c => c.PartDefinition.Name == ContentTypes.ProjectProjectionContentType);
                if (projectProjectionPart != null)
                {
                    var field = projectProjectionPart.Fields.FirstOrDefault(c => c.Name == FieldNames.ProjectProjectionItemTypeFieldName);
                    if (field != null)
                    {
                        ((InputField)field).Value = itemContentType;
                    }

                    var displayField = projectProjectionPart.Fields.FirstOrDefault(c => c.Name == FieldNames.ProjectProjectionItemTypeDisplayFieldName);
                    if (displayField != null)
                    {
                        var contentTypeDefinition = this.contentManager.GetContentTypeDefinitions().FirstOrDefault(c => c.Name == itemContentType);
                        ((InputField)displayField).Value = contentTypeDefinition.DisplayName;
                    }
                }

                this.contentManager.Publish(contentItem);

                return contentItem;
            }

            return null;
        }

        public QueryPart CreateQuery(string title, string contentType)
        {
            var queryPart = this.GetQuery(title);

            if (queryPart != null)
            {
                return queryPart;
            }

            var query = this.contentManager.Create("Query");
            query.As<TitlePart>().Title = title;
            this.contentManager.Publish(query);
            queryPart = query.As<QueryPart>();
            var filterGroup = queryPart.Record.FilterGroups.FirstOrDefault();

            // ticket ContentType filter
            this.CreateContentTypeFilter(contentType, filterGroup);

            return queryPart;
        }

        public QueryPart CreateQuery(string title, string contentType, string shapeName, string layoutName, bool createDefaultSortCriteria, bool filterToItemsVisibleByCurrentUser)
        {
            return CreateQuery(title, contentType, shapeName, layoutName, "Summary", createDefaultSortCriteria, filterToItemsVisibleByCurrentUser);
        }

        public QueryPart CreateQuery(string title, string contentType, string shapeName, string layoutName, string itemDisplayType, bool createDefaultSortCriteria, bool filterToItemsVisibleByCurrentUser)
        {
            var queryPart = this.CreateQuery(title, contentType);
            var filterGroup = queryPart.Record.FilterGroups.FirstOrDefault();

            // All items visible by current user
            if (filterToItemsVisibleByCurrentUser)
            {
                string state = string.Empty;
                this.CreateFilter(ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.CurrentUserPermissions, state, filterGroup);
            }

            if (createDefaultSortCriteria)
            {
                this.CreateSortRecord("CommonPartRecord", "PublishedUtc", "<Form><Sort>false</Sort></Form>", queryPart.Record);
            }

            string layoutState = string.Format(
                CultureInfo.InvariantCulture,
                "<Form><Category>Html</Category><Type>{0}</Type><Display>0</Display><DisplayType>{1}</DisplayType><ShapeType>{2}</ShapeType></Form>",
                layoutName,
                itemDisplayType,
                shapeName);

            AddLayout(layoutState, itemDisplayType, queryPart);

            return queryPart;
        }

        public LayoutRecord AddLayout(string layoutState, string itemDisplayType, QueryPart queryPart)
        {
            // create layout
            LayoutRecord layout = new LayoutRecord
            {
                Type = "Shape",
                Category = "Html",
                QueryPartRecord = queryPart.Record,
                DisplayType = itemDisplayType,
                Display = 0,
                State = layoutState
            };

            this.layoutRepository.Create(layout);
            this.layoutRepository.Flush();
            this.filterRepository.Flush();

            return layout;
        }
    }
}