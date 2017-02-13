using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.CRM.Core.Services
{
    public class ExtendedContentManager : IExtendedContentManager
    {
        private readonly IComponentContext _context;
        private readonly IRepository<ContentTypeRecord> _contentTypeRepository;
        private readonly IRepository<ContentItemRecord> _contentItemRepository;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ICacheManager _cacheManager;
        private readonly IPartTypeRecordMatchingService partTypeRecordMatchingService;
        private readonly Func<IContentManagerSession> _contentManagerSession;
        private readonly Lazy<IContentDisplay> _contentDisplay;
        private readonly Lazy<ISessionLocator> _sessionLocator;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly Lazy<IEnumerable<IIdentityResolverSelector>> _identityResolverSelectors;
        private readonly Lazy<IEnumerable<ISqlStatementProvider>> _sqlStatementProviders;
        private readonly ShellSettings _shellSettings;
        private readonly ISignals _signals;
        private IContentManager contentManager;

        private const string Published = "Published";
        private const string Draft = "Draft";

        public ExtendedContentManager(
            IComponentContext context,
            IRepository<ContentTypeRecord> contentTypeRepository,
            IRepository<ContentItemRecord> contentItemRepository,
            IRepository<ContentItemVersionRecord> contentItemVersionRepository,
            IContentDefinitionManager contentDefinitionManager,
            ICacheManager cacheManager,
            Func<IContentManagerSession> contentManagerSession,
            Lazy<IContentDisplay> contentDisplay,
            IPartTypeRecordMatchingService partTypeRecordMatchingService,
            Lazy<ISessionLocator> sessionLocator,
            Lazy<IEnumerable<IContentHandler>> handlers,
            Lazy<IEnumerable<IIdentityResolverSelector>> identityResolverSelectors,
            Lazy<IEnumerable<ISqlStatementProvider>> sqlStatementProviders,
            ShellSettings shellSettings,
            ISignals signals,
            IContentManager contentManager)
        {
            this.partTypeRecordMatchingService = partTypeRecordMatchingService;
            this.contentManager = contentManager;
            _context = context;
            _contentTypeRepository = contentTypeRepository;
            _contentItemRepository = contentItemRepository;
            _contentItemVersionRepository = contentItemVersionRepository;
            _contentDefinitionManager = contentDefinitionManager;
            _cacheManager = cacheManager;
            _contentManagerSession = contentManagerSession;
            _identityResolverSelectors = identityResolverSelectors;
            _sqlStatementProviders = sqlStatementProviders;
            _shellSettings = shellSettings;
            _signals = signals;
            _handlers = handlers;
            _contentDisplay = contentDisplay;
            _sessionLocator = sessionLocator;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<IContentHandler> Handlers
        {
            get { return _handlers.Value; }
        }

        public virtual ContentItem Get(int id, string contentType)
        {
            return this.Get(id, VersionOptions.Published, contentType);
        }

        public virtual ContentItem Get(int id, VersionOptions options, string contentType)
        {
            return this.Get(id, options, QueryHints.Empty, contentType);
        }

        public virtual ContentItem Get(int id, VersionOptions options, QueryHints hints, string contentType)
        {
            ContentItem contentItem;

            ContentItemVersionRecord versionRecord = null;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (contentTypeDefinition == null)
            {
                contentTypeDefinition = new ContentTypeDefinitionBuilder().Named(contentType).Build();
            }

            if (hints == QueryHints.Empty)
            {
                hints = new QueryHints();
            }

            hints = hints.ExpandRecords(contentTypeDefinition.Parts.Select(c => c.PartDefinition.Name + "Record"));

            // do a query to load the records in case Get is called directly
            contentItem = GetContentItem(hints, contentTypeDefinition,
                (contentItemCriteria, contentItemVersionCriteria) =>
                {
                    contentItemCriteria.Add(Restrictions.Eq("Id", id));
                    if (options.IsPublished)
                    {
                        contentItemVersionCriteria.Add(Restrictions.Eq("Published", true));
                    }
                    else if (options.IsLatest)
                    {
                        contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
                    }
                    else if (options.IsDraft && !options.IsDraftRequired)
                    {
                        contentItemVersionCriteria.Add(
                            Restrictions.And(Restrictions.Eq("Published", false),
                                            Restrictions.Eq("Latest", true)));
                    }
                    else if (options.IsDraft || options.IsDraftRequired)
                    {
                        contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
                    }

                    if (options.VersionNumber != default(int))
                    {
                        contentItemVersionCriteria.Add(Restrictions.Eq("VersionNumber", options.VersionNumber));
                    }

                    contentItemVersionCriteria.SetFetchMode("ContentItemRecord", FetchMode.Eager);
                    contentItemVersionCriteria.SetFetchMode("ContentItemRecord.ContentType", FetchMode.Eager);
                    contentItemVersionCriteria.SetMaxResults(1);
                });

            FinalizeContentItem(contentItem, options, contentType);

            return contentItem;
        }

        public virtual void FinalizeContentItem(ContentItem contentItem, VersionOptions options, string contentType)
        {
            var session = _contentManagerSession();

            // store in session prior to loading to avoid some problems with simple circular dependencies
            session.Store(contentItem);

            // create a context with a new instance to load            
            var context = new LoadContentContext(contentItem);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Loading(context), Logger);
            Handlers.Invoke(handler => handler.Loaded(context), Logger);

            // when draft is required and latest is published a new version is appended 
            if (options.IsDraftRequired)// && versionRecord.Published)
            {
                contentItem = BuildNewVersion(context.ContentItem, contentType);
            }
        }

        public virtual ContentItem New(string contentType)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (contentTypeDefinition == null)
            {
                contentTypeDefinition = new ContentTypeDefinitionBuilder().Named(contentType).Build();
            }

            // create a new kernel for the model instance
            var context = new ActivatingContentContext
            {
                ContentType = contentTypeDefinition.Name,
                Definition = contentTypeDefinition,
                Builder = new ContentItemBuilder(contentTypeDefinition)
            };

            // invoke handlers to weld aspects onto kernel
            Handlers.Invoke(handler => handler.Activating(context), Logger);

            var context2 = new ActivatedContentContext
            {
                ContentType = contentType,
                ContentItem = context.Builder.Build()
            };

            // back-reference for convenience (e.g. getting metadata when in a view)
            context2.ContentItem.ContentManager = this.contentManager;

            Handlers.Invoke(handler => handler.Activated(context2), Logger);

            var context3 = new InitializingContentContext
            {
                ContentType = context2.ContentType,
                ContentItem = context2.ContentItem,
            };

            Handlers.Invoke(handler => handler.Initializing(context3), Logger);

            // composite result is returned
            return context3.ContentItem;
        }
        protected virtual ContentItem BuildNewVersion(ContentItem existingContentItem, string contentType)
        {
            var contentItemRecord = existingContentItem.Record;

            // locate the existing and the current latest versions, allocate building version
            var existingItemVersionRecord = existingContentItem.VersionRecord;
            var buildingItemVersionRecord = new ContentItemVersionRecord
            {
                ContentItemRecord = contentItemRecord,
                Latest = true,
                Published = false,
                Data = existingItemVersionRecord.Data,
            };


            var latestVersion = contentItemRecord.Versions.SingleOrDefault(x => x.Latest);

            if (latestVersion != null)
            {
                latestVersion.Latest = false;
                buildingItemVersionRecord.Number = latestVersion.Number + 1;
            }
            else
            {
                buildingItemVersionRecord.Number = contentItemRecord.Versions.Max(x => x.Number) + 1;
            }

            contentItemRecord.Versions.Add(buildingItemVersionRecord);
            _contentItemVersionRepository.Create(buildingItemVersionRecord);

            var buildingContentItem = New(contentType);
            buildingContentItem.VersionRecord = buildingItemVersionRecord;

            var context = new VersionContentContext
            {
                Id = existingContentItem.Id,
                ContentType = existingContentItem.ContentType,
                ContentItemRecord = contentItemRecord,
                ExistingContentItem = existingContentItem,
                BuildingContentItem = buildingContentItem,
                ExistingItemVersionRecord = existingItemVersionRecord,
                BuildingItemVersionRecord = buildingItemVersionRecord,
            };

            Handlers.Invoke(handler => handler.Versioning(context), Logger);
            Handlers.Invoke(handler => handler.Versioned(context), Logger);

            return context.BuildingContentItem;
        }

        private ContentItem GetContentItem(QueryHints hints, ContentTypeDefinition contentTypeDefinition, Action<ICriteria, ICriteria> predicate)
        {
            var session = _sessionLocator.Value.For(typeof(ContentItemRecord));
            var contentItemVersionCriteria = session.CreateCriteria(typeof(ContentItemVersionRecord));
            var contentItemCriteria = contentItemVersionCriteria.CreateCriteria("ContentItemRecord");
            predicate(contentItemCriteria, contentItemVersionCriteria);

            var contentItemMetadata = session.SessionFactory.GetClassMetadata(typeof(ContentItemRecord));
            var contentItemVersionMetadata = session.SessionFactory.GetClassMetadata(typeof(ContentItemVersionRecord));

            if (hints != QueryHints.Empty)
            {
                // break apart and group hints by their first segment
                var hintDictionary = hints.Records
                    .Select(hint => new { Hint = hint, Segments = hint.Split('.') })
                    .GroupBy(item => item.Segments.FirstOrDefault())
                    .ToDictionary(grouping => grouping.Key, StringComparer.InvariantCultureIgnoreCase);

                // locate hints that match properties in the ContentItemVersionRecord
                foreach (var hit in contentItemVersionMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key]))
                {
                    contentItemVersionCriteria.SetFetchMode(hit.Hint, FetchMode.Eager);
                    hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemVersionCriteria, ExtendCriteria);
                }

                // locate hints that match properties in the ContentItemRecord
                foreach (var hit in contentItemMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key]))
                {
                    contentItemVersionCriteria.SetFetchMode("ContentItemRecord." + hit.Hint, FetchMode.Eager);
                    hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemCriteria, ExtendCriteria);
                }

                if (hintDictionary.SelectMany(x => x.Value).Any(x => x.Segments.Count() > 1))
                    contentItemVersionCriteria.SetResultTransformer(new DistinctRootEntityResultTransformer());
            }

            contentItemCriteria.SetCacheable(true);

            ContentItemTransformer contentItemTransformer = new ContentItemTransformer(contentTypeDefinition, this.partTypeRecordMatchingService, this.New);
            var returnValue = contentItemVersionCriteria.SetResultTransformer(contentItemTransformer).List<ContentItem>();

            return returnValue.FirstOrDefault();
        }

        private static ICriteria ExtendCriteria(ICriteria criteria, string segment)
        {
            return criteria.GetCriteriaByPath(segment) ?? criteria.CreateCriteria(segment, JoinType.LeftOuterJoin);
        }

        private ContentItemVersionRecord GetVersionRecord(VersionOptions options, ContentItemRecord itemRecord)
        {
            if (options.IsPublished)
            {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Published) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Published);
            }
            if (options.IsLatest || options.IsDraftRequired)
            {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Latest) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Latest);
            }
            if (options.IsDraft)
            {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Latest && !x.Published) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Latest && !x.Published);
            }
            if (options.VersionNumber != 0)
            {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Number == options.VersionNumber) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Number == options.VersionNumber);
            }
            return null;
        }

    }
}