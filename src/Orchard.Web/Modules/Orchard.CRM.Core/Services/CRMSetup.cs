using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Services;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Settings.Metadata.Records;
using Orchard.CRM.Core.Commands;
using Orchard.CRM.Core.Models;
using Orchard.Data;
using Orchard.Indexing;
using Orchard.Indexing.Services;
using Orchard.Projections.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tasks.Indexing;
using Orchard.UI.Navigation;
using Orchard.Workflows.Models;
using System;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Services
{
    public class CRMSetup : ICRMSetup
    {
        public BasicDataCommand OperatorMenuCommand;

        private readonly IContentManager contentManager;
        private readonly IMembershipService membershipService;
        private readonly ISiteService siteService;
        private readonly Orchard.Widgets.Services.IWidgetsService widgetService;
        private readonly IRepository<PropertyRecord> propertyRecordRepository;
        private readonly INavigationManager navigationManager;
        private readonly IAuthenticationService authenticationService;
        private readonly IContentDefinitionService contentDefinitionService;
        private readonly IRepository<ContentTypeDefinitionRecord> contentTypeDefinitionRecordRepository;

        public CRMSetup(
            IContentDefinitionManager contentDefinitionManager,
            IRepository<StatusRecord> statusRepository,
            IRepository<PriorityRecord> priorityRepository,
            IRepository<TicketTypeRecord> ticketTypeRepository,
            IOrchardServices orchardServices,
            IRepository<ContentTypeDefinitionRecord> contentTypeDefinitionRecordRepository,
            IContentDefinitionService contentDefinitionService,
            Orchard.Widgets.Services.IWidgetsService widgetService,
            IContentManager contentManager,
            IMembershipService membershipService,
            IAuthenticationService authenticationService,
            ISiteService siteService,
            IMenuService menuService,
            IRepository<PropertyRecord> propertyRecordRepository,
            INavigationManager navigationManager,
            IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository,
            IRepository<ActivityRecord> activityRepository,
            IRepository<EmailTemplateRecord> emailTemplatesRepository,
            IRepository<TransitionRecord> transitionRepository,
            IIndexManager indexManager)
        {
            this.widgetService = widgetService;
            this.contentTypeDefinitionRecordRepository = contentTypeDefinitionRecordRepository;
            this.contentDefinitionService = contentDefinitionService;
            this.propertyRecordRepository = propertyRecordRepository;
            this.contentManager = contentManager;
            this.membershipService = membershipService;
            this.siteService = siteService;
            this.navigationManager = navigationManager;
            this.authenticationService = authenticationService;

              this.OperatorMenuCommand = new BasicDataCommand(
                contentDefinitionManager,
                contentDefinitionService,
                orchardServices,
                this.contentManager,
                this.navigationManager,
                this.membershipService,
                this.authenticationService,
                this.siteService,
                workflowDefinitionRepository,
                activityRepository,
                ticketTypeRepository,
                transitionRepository,
                statusRepository,
                emailTemplatesRepository,
                priorityRepository,
                indexManager);
        }

        public void AddBasicData()
        {
             this.OperatorMenuCommand.AddCRMBasicData();
        }
    }
}