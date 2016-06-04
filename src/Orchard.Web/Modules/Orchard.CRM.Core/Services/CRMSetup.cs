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