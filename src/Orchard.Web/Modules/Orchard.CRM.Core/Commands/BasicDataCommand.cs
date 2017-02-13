namespace Orchard.CRM.Core.Commands
{
    using Orchard.Commands;
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.MetaData;
    using Orchard.ContentTypes.Services;
    using Orchard.ContentTypes.ViewModels;
    using Orchard.CRM.Core.Activities;
    using Orchard.CRM.Core.Controllers;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers;
    using Orchard.Data;
    using Orchard.Indexing;
    using Orchard.Security;
    using Orchard.Settings;
    using Orchard.UI.Navigation;
    using Orchard.Workflows.Models;
    using S22.IMAP.Activities;
    using System;
    using System.Globalization;
    using System.Linq;

    public class BasicDataCommand : DefaultOrchardCommandHandler
    {
        private readonly IIndexManager indexManager;
        private readonly IContentDefinitionManager contentDefinitionManager;
        private readonly ISiteService siteService;
        private readonly IAuthenticationService authenticationService;
        private readonly IRepository<StatusRecord> statusRepository;
        private readonly IRepository<TicketTypeRecord> ticketTypeRepository;
        private readonly IRepository<PriorityRecord> priorityRepository;
        private readonly IRepository<EmailTemplateRecord> emailTemplatesRepository;
        private readonly IMembershipService membershipService;
        private readonly IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository;
        private readonly IRepository<ActivityRecord> activityRepository;
        private readonly IRepository<TransitionRecord> transitionRepository;
        protected readonly IContentManager contentManager;
        private readonly IContentDefinitionService contentDefinitionService;

        public const string ServicesQueryName = "Services";
        public const string ServicesProjectionName = "Services Projection";

        public const string MyTicketsQueryName = "My Tickets";
        public const string MyTicketsProjectionName = "My Tickets Projection";

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public string CreateTicketCommentsWorkflowName
        {
            get
            {
                return this.createTicketCommentsWorkflowName;
            }
            set
            {
                this.createTicketCommentsWorkflowName = value;
            }
        }
        private string createTicketCommentsWorkflowName = "Create Ticket Comments Workflow";

        [OrchardSwitch]
        public string SendEmailToAssignee
        {
            get
            {
                return this.sendEmailToAssignee;
            }
            set
            {
                this.sendEmailToAssignee = value;
            }
        }
        private string sendEmailToAssignee = "Send Email to assignee";

        [OrchardSwitch]
        public string SendEmailForCreatedTicket
        {
            get
            {
                return this.sendEmailForCreatedTicket;
            }
            set
            {
                this.sendEmailForCreatedTicket = value;
            }
        }
        private string sendEmailForCreatedTicket = "Send Email for created Ticket";

        [OrchardSwitch]
        public string CreateTicketForRecievedEmailWorkflowName
        {
            get
            {
                return this.createTicketForRecievedEmailWorkflowName;
            }
            set
            {
                this.createTicketForRecievedEmailWorkflowName = value;
            }
        }
        private string createTicketForRecievedEmailWorkflowName = "Create Ticket For received Emails";

        public BasicDataCommand(
            IContentDefinitionManager contentDefinitionManager,
            IContentDefinitionService contentDefinitionService,
            IOrchardServices services,
            IContentManager contentManager,
            INavigationManager navigationManager,
            IMembershipService membershipService,
            IAuthenticationService authenticationService,
            ISiteService siteService,
            IRepository<WorkflowDefinitionRecord> workflowDefinitionRepository,
            IRepository<ActivityRecord> activityRepository,
            IRepository<TicketTypeRecord> ticketTypeRepository,
            IRepository<TransitionRecord> transitionRepository,
            IRepository<StatusRecord> statusRepository,
            IRepository<EmailTemplateRecord> emailTemplatesRepository,
            IRepository<PriorityRecord> priorityRepository,
            IIndexManager indexManager)
        {
            this.contentDefinitionService = contentDefinitionService;
            this.indexManager = indexManager;
            this.contentDefinitionManager = contentDefinitionManager;
            this.emailTemplatesRepository = emailTemplatesRepository;
            this.transitionRepository = transitionRepository;
            this.activityRepository = activityRepository;
            this.workflowDefinitionRepository = workflowDefinitionRepository;
            this.siteService = siteService;
            this.authenticationService = authenticationService;
            this.membershipService = membershipService;
            this.statusRepository = statusRepository;
            this.ticketTypeRepository = ticketTypeRepository;
            this.priorityRepository = priorityRepository;
            this.contentManager = contentManager;
        }

        [CommandName("add crm-basic-data")]
        [OrchardSwitches("Owner")]
        public void AddCRMBasicData()
        {
            if (String.IsNullOrEmpty(this.Owner))
            {
                this.Owner = this.siteService.GetSiteSettings().SuperUser;
            }

            IUser owner = this.membershipService.GetUser(Owner);
            this.authenticationService.SetAuthenticatedUserForRequest(owner);

            // fill basic data tables
            this.CreatePriorityRecords();
            this.CreateStatusRecords();
            this.CreateTicketTypeRecords();
            this.CreateEmailTemplates();
            this.CreateBusinessUnits();
            this.AddFullNameToUserPart();

            // Create index
            this.indexManager.GetSearchIndexProvider().CreateIndex(TicketController.SearchIndexName);

            // Index the ticket items in the create Index
            var ticketTypeDefinition = this.contentDefinitionManager.GetTypeDefinition("Ticket");
            ticketTypeDefinition.Settings["TypeIndexing.Indexes"] = TicketController.SearchIndexName;
            this.contentDefinitionManager.StoreTypeDefinition(ticketTypeDefinition);

            // Index CRM comment items in the create Index
            var commentTypeDefinition = this.contentDefinitionManager.GetTypeDefinition("CRMComment");
            commentTypeDefinition.Settings["TypeIndexing.Indexes"] = TicketController.SearchIndexName;
            this.contentDefinitionManager.StoreTypeDefinition(commentTypeDefinition);

            // workflow
            this.CreateTicketAssignmentWorkflow();
            this.CreateCRMCommentWorkflow();
            this.CreateTicketCreationWorkflow();
            this.CreateTicketForReceivedEmailsWorkflow();
        }

        private void AddFullNameToUserPart()
        {
            string id = "User";
            string fieldName = "FullName";
            string displayName = "Full Name";
            string fieldTypeName = "InputField";

            var partViewModel = this.contentDefinitionService.GetPart(id);
            var typeViewModel = this.contentDefinitionService.GetType(id);
            if (partViewModel == null)
            {
                // id passed in might be that of a type w/ no implicit field
                partViewModel = new EditPartViewModel { Name = typeViewModel.Name };
                this.contentDefinitionService.AddPart(new CreatePartViewModel { Name = partViewModel.Name });
                this.contentDefinitionService.AddPartToType(partViewModel.Name, typeViewModel.Name);
                partViewModel = this.contentDefinitionService.GetPart(id);
            }

            if (partViewModel.Fields.Any(t => String.Equals(t.Name.Trim(), fieldName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            this.contentDefinitionService.AddFieldToPart(fieldName, displayName, fieldTypeName, id);
        }

        private void CreateStatusRecords()
        {
            var statusRecords = this.statusRepository.Table.ToList();

            var statusNames = new Tuple<int, string>[] {
                new Tuple<int,string>(StatusRecord.NewStatus, "New"), 
                new Tuple<int,string>(StatusRecord.OpenStatus, "In Progress"), 
                new Tuple<int,string>(StatusRecord.DeferredStatus, "Deferred"), 
                new Tuple<int,string>(StatusRecord.PendingInputStatus, "Pending input"), 
                new Tuple<int,string>(StatusRecord.ClosedStatus, "Completed")};

            for (int i = 0; i < statusNames.Length; i++)
            {
                StatusRecord statusRecord = statusRecords.FirstOrDefault(c => c.StatusTypeId == statusNames[i].Item1);
                if (statusRecord == null)
                {
                    statusRecord = new StatusRecord
                    {
                        OrderId = i + 1,
                        Name = statusNames[i].Item2,
                        StatusTypeId = statusNames[i].Item1,
                        IsHardCode = true
                    };

                    this.statusRepository.Create(statusRecord);
                }
            }

            this.statusRepository.Flush();
        }

        private void CreateEmailTemplates()
        {
            var emailTemplates = this.emailTemplatesRepository.Table.ToList();

            // Ticket
            EmailTemplateRecord newTicketEmailTemplate = emailTemplates.FirstOrDefault(c => c.TypeId == (int)EmailTemplateType.TicketAssignedToUser);
            if (newTicketEmailTemplate == null)
            {
                newTicketEmailTemplate = new EmailTemplateRecord
                {
                    TypeId = (int)EmailTemplateType.TicketAssignedToUser,
                    Subject = "New Ticket {ETicket.Title}",
                    Body = @"<p>Dear {EUser.FullName}</p>
<p>It is to let you know that the ticket '{ETicket.Title}' has been assigned to you.</p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Requesting User:</span> <span>{ETicket.RequestingUserFullName}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Title:</span> <span>{ETicket.Title}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Priority:</span> <span>{ETicket.Priority}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Service:</span> <span>{ETicket.Service}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Ticket Type:</span> <span>{ETicket.TicketType}</span></p>
<p>Orchard Collaboration System</p>",
                    Name = "Ticket Assignment EmailTemplate"
                };

                this.emailTemplatesRepository.Create(newTicketEmailTemplate);
            }

            // Ticket
            EmailTemplateRecord followerEmailTemplate = emailTemplates.FirstOrDefault(c => c.TypeId == (int)EmailTemplateType.FollowersNotification);
            if (followerEmailTemplate == null)
            {
                followerEmailTemplate = new EmailTemplateRecord
                {
                    TypeId = (int)EmailTemplateType.FollowersNotification,
                    Subject = "New Changes in {EActivityStream.ContentDescription}",
                    Body = @"<p>Dear {EUser.FullName}</p>
                        <p>It is to let you know that the following changes has been applied to <a href='{EActivityStream.Url}'>'{EActivityStream.ContentDescription}'</a></p>
                        <p><span style='display: inline-block; width: 150px; color: gray;'>Changed by:</span> <span>{EActivityStream.ChangedByFullname}</span></p>
                        <p><span style='display: inline-block; width: 150px; color: gray;'>Description:</span> <span>{EActivityStream.ChangeDescription}</span></p>
                        <p>{EActivityStream.Changes}</p>
                        <p>Orchard Collaboration System</p>",
                    Name = "New Changes Notification"
                };

                this.emailTemplatesRepository.Create(followerEmailTemplate);
            }

            
            // CRM Comment
            EmailTemplateRecord commentEmailTemplate = emailTemplates.FirstOrDefault(c => c.TypeId == (int)EmailTemplateType.NewMessage);
            if (commentEmailTemplate == null)
            {
                commentEmailTemplate = new EmailTemplateRecord
                {
                    TypeId = (int)EmailTemplateType.NewMessage,
                    Subject = "New Comment {ETitle.Title}",
                    Body = @"<p>Dear {EUser.FullName}</p>
<p>It is to let you know that a new comment for the ticket '{ETicket.Title}' has been written.</p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Requesting User:</span> <span>{ETicket.RequestingUserFullName}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Title:</span> <span>{ETicket.Title}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Priority:</span> <span>{ETicket.Priority}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Service:</span> <span>{ETicket.Service}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Ticket Type:</span> <span>{ETicket.TicketType}</span></p>
<p>Orchard Collaboration System</p>",
                    Name = "New Message EmailTemplate"
                };

                this.emailTemplatesRepository.Create(commentEmailTemplate);
            }

            // 
            EmailTemplateRecord createTicketConfirmationTemplate = emailTemplates.FirstOrDefault(c => c.TypeId == (int)EmailTemplateType.TicketCreated);
            if (createTicketConfirmationTemplate == null)
            {
                createTicketConfirmationTemplate = new EmailTemplateRecord
                {
                    TypeId = (int)EmailTemplateType.TicketCreated,
                    Subject = "New Ticket Confirmation {ETitle.Title}",
                    Body = @"<p>Dear {EUser.FullName}</p>
<p>It is to let you know that the ticket '{ETicket.Title}' has been created successfully.</p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Requesting User:</span> <span>{ETicket.RequestingUserFullName}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Title:</span> <span>{ETicket.Title}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Priority:</span> <span>{ETicket.Priority}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Service:</span> <span>{ETicket.Service}</span></p>
<p><span style='display: inline-block; width: 150px; color: gray;'>Ticket Type:</span> <span>{ETicket.TicketType}</span></p>
<p>Orchard Collaboration System</p>",
                    Name = "Ticket creation EmailTemplate"
                };

                this.emailTemplatesRepository.Create(createTicketConfirmationTemplate);
            }

            this.emailTemplatesRepository.Flush();
        }

        private void CreateTicketTypeRecords()
        {
            var ticketTypeRecords = this.ticketTypeRepository.Table.ToList();
            string[] ticketTypes = new string[] { "Issue", "Request", "Error", "Task" };

            int counter = 1;
            foreach (var item in ticketTypes.Where(d => !ticketTypeRecords.Any(c => c.Name.ToLower(CultureInfo.InvariantCulture) == d.ToLower(CultureInfo.InvariantCulture))))
            {
                var record = new TicketTypeRecord { Name = item, OrderId = counter };
                counter++;
                this.ticketTypeRepository.Create(record);
            }

            this.ticketTypeRepository.Flush();
        }

        private void CreateBusinessUnits()
        {
            string[] businessUnits = new string[] { "Sales", "Support" };

            foreach (var item in businessUnits)
            {
                var businessUnit = this.contentManager.New("BusinessUnit");
                this.contentManager.Create(businessUnit);
                BusinessUnitPart part = businessUnit.As<BusinessUnitPart>();
                BusinessUnitPartRecord businessUnitRecord = part.Record;

                businessUnitRecord.Name = item;
                businessUnitRecord.Description = string.Empty;
                this.contentManager.Publish(businessUnit);
            }

            this.ticketTypeRepository.Flush();
        }

        private void CreatePriorityRecords()
        {
            var priorityRecords = this.priorityRepository.Table.ToList();

            string[] priorityNames = new string[] { PriorityRecord.LowPriority, PriorityRecord.NormalPriority, PriorityRecord.HighPriority, PriorityRecord.CriticalPriority };

            for (int i = 0; i < priorityNames.Length; i++)
            {
                PriorityRecord priorityRecord = priorityRecords.FirstOrDefault(c => c.Name == priorityNames[i]);
                if (priorityRecord != null)
                {
                    priorityRecord.OrderId = i + 1;
                    priorityRecord.IsHardCode = true;
                }
                else
                {
                    priorityRecord = new PriorityRecord
                    {
                        OrderId = i + 1,
                        Name = priorityNames[i],
                        IsHardCode = true
                    };

                    this.priorityRepository.Create(priorityRecord);
                }
            }

            this.priorityRepository.Flush();
        }

        private void CreateTicketAssignmentWorkflow()
        {
            WorkflowDefinitionRecord workflow = new WorkflowDefinitionRecord { Name = this.SendEmailToAssignee, Enabled = true };
            this.workflowDefinitionRepository.Create(workflow);
            var emailTemplate = this.emailTemplatesRepository.Table.FirstOrDefault(c => c.TypeId == (int)EmailTemplateType.TicketAssignedToUser);

            if (emailTemplate == null)
            {
                throw new NullReferenceException();
            }

            ActivityRecord ticketPublishRecord = new ActivityRecord
            {
                WorkflowDefinitionRecord = workflow,
                Start = true,
                X = 552,
                Y = 227,
                State = "{\"ContentTypes\":\"Ticket\"}",
                Name = "ContentCreated"
            };
            this.activityRepository.Create(ticketPublishRecord);

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

            ActivityRecord sendTicketEmailActivityRecord = new ActivityRecord
            {
                State = string.Format(
                   CultureInfo.InvariantCulture,
                "{{\"{0}\":\"{1}\",\"{2}\":\"true\"}}",
                   EmailTemplateActivityForm.EmailTemplateIdFieldName,
                   emailTemplate.Id.ToString(CultureInfo.InvariantCulture),
                   EmailTemplateActivityForm.SentToOwnerFieldName),
                Name = "SendTicketEmailActivity",
                WorkflowDefinitionRecord = workflow,
                X = 494,
                Y = 588,
                Start = false
            };

            this.activityRepository.Create(sendTicketEmailActivityRecord);

            TransitionRecord timerTransitionRecord = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = timerActivity,
                SourceActivityRecord = ticketPublishRecord,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(timerTransitionRecord);

            TransitionRecord sendTicketEmailTransitionRecord = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = sendTicketEmailActivityRecord,
                SourceActivityRecord = timerActivity,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(sendTicketEmailTransitionRecord);

            this.workflowDefinitionRepository.Flush();
            this.activityRepository.Flush();
            this.transitionRepository.Flush();
        }

        private void CreateTicketCreationWorkflow()
        {
            WorkflowDefinitionRecord workflow = new WorkflowDefinitionRecord { Name = this.SendEmailForCreatedTicket, Enabled = true };
            this.workflowDefinitionRepository.Create(workflow);
            var emailTemplate = this.emailTemplatesRepository.Table.FirstOrDefault(c => c.TypeId == (int)EmailTemplateType.TicketCreated);

            if (emailTemplate == null)
            {
                throw new NullReferenceException();
            }

            ActivityRecord ticketPublishRecord = new ActivityRecord
            {
                WorkflowDefinitionRecord = workflow,
                Start = true,
                X = 552,
                Y = 227,
                State = "{\"ContentTypes\":\"Ticket\"}",
                Name = "ContentCreated"
            };
            this.activityRepository.Create(ticketPublishRecord);

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

            ActivityRecord sendTicketEmailActivityRecord = new ActivityRecord
            {
                State = string.Format(
                   CultureInfo.InvariantCulture,
                "{{\"{0}\":\"{1}\",\"{2}\":\"true\",\"{3}\":\"false\"}}",
                   EmailTemplateActivityForm.EmailTemplateIdFieldName,
                   emailTemplate.Id.ToString(CultureInfo.InvariantCulture),
                   EmailTemplateActivityForm.SentToRequestingUserFieldName,
                   EmailTemplateActivityForm.SentToOwnerFieldName),
                Name = "SendTicketEmailActivity",
                WorkflowDefinitionRecord = workflow,
                X = 494,
                Y = 588,
                Start = false
            };

            this.activityRepository.Create(sendTicketEmailActivityRecord);

            TransitionRecord timerTransitionRecord = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = timerActivity,
                SourceActivityRecord = ticketPublishRecord,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(timerTransitionRecord);

            TransitionRecord sendTicketEmailTransitionRecord = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = sendTicketEmailActivityRecord,
                SourceActivityRecord = timerActivity,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(sendTicketEmailTransitionRecord);

            this.workflowDefinitionRepository.Flush();
            this.activityRepository.Flush();
            this.transitionRepository.Flush();
        }

        private void CreateTicketForReceivedEmailsWorkflow()
        {
            WorkflowDefinitionRecord workflow = new WorkflowDefinitionRecord { Name = this.CreateTicketForRecievedEmailWorkflowName, Enabled = true };
            this.workflowDefinitionRepository.Create(workflow);

            ActivityRecord emailReceivedRecord = new ActivityRecord
            {
                WorkflowDefinitionRecord = workflow,
                Start = true,
                State = "{}",
                X = 552,
                Y = 227,
                Name = IMapEmailReceivedActivity.ActivityName
            };
            this.activityRepository.Create(emailReceivedRecord);

            ActivityRecord checkEmailBelongToUserActivityRecord = new ActivityRecord
            {
                State = "{}",
                Name = "CheckEmailBelongToUser",
                WorkflowDefinitionRecord = workflow,
                X = 494,
                Y = 538,
                Start = false
            };

            this.activityRepository.Create(checkEmailBelongToUserActivityRecord);

            TransitionRecord transitionRecord1 = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = checkEmailBelongToUserActivityRecord,
                SourceActivityRecord = emailReceivedRecord,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(transitionRecord1);
            
            ActivityRecord createTicketActivityRecord = new ActivityRecord
            {
                State = "{\"EditableBy\":\"{EEmail.UserName}\",\"Title\":\"{EEmail.Subject}\",\"Description\":\"{EEmail.Body}\",\"DownloadEmailAttachments\":\"true\"}",
                Name = CreateTicketActivity.ActivityName,
                WorkflowDefinitionRecord = workflow,
                X = 494,
                Y = 688,
                Start = false
            };

            this.activityRepository.Create(createTicketActivityRecord);

            TransitionRecord transitionRecord2 = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = createTicketActivityRecord,
                SourceActivityRecord = checkEmailBelongToUserActivityRecord,
                DestinationEndpoint = "",                
                SourceEndpoint = "Yes"
            };
            this.transitionRepository.Create(transitionRecord2);

            this.workflowDefinitionRepository.Flush();
            this.activityRepository.Flush();
            this.transitionRepository.Flush();
        }

        private void CreateCRMCommentWorkflow()
        {
            WorkflowDefinitionRecord workflow = new WorkflowDefinitionRecord { Name = this.CreateTicketCommentsWorkflowName, Enabled = true };
            this.workflowDefinitionRepository.Create(workflow);
            var emailTemplate = this.emailTemplatesRepository.Table.FirstOrDefault(c => c.TypeId == (int)EmailTemplateType.NewMessage);

            if (emailTemplate == null)
            {
                throw new NullReferenceException();
            }

            ActivityRecord crmCommentPublishRecord = new ActivityRecord
            {
                WorkflowDefinitionRecord = workflow,
                Start = true,
                X = 552,
                Y = 227,
                State = "{\"ContentTypes\":\"CRMComment\"}",
                Name = "ContentPublished"
            };
            this.activityRepository.Create(crmCommentPublishRecord);

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

            ActivityRecord sendEmailCRMCommentActivity = new ActivityRecord
            {
                State = string.Format(
                CultureInfo.InvariantCulture,
                "{{\"{0}\":\"{1}\",\"{2}\":\"true\",\"{3}\":\"true\"}}",
                EmailTemplateActivityForm.EmailTemplateIdFieldName,
                emailTemplate.Id.ToString(CultureInfo.InvariantCulture),
                EmailTemplateActivityForm.SentToRequestingUserFieldName,
                EmailTemplateActivityForm.SentToOwnerFieldName
                ),
                Name = "SendTicketEmailActivity",
                WorkflowDefinitionRecord = workflow,
                X = 494,
                Y = 588,
                Start = false
            };

            this.activityRepository.Create(sendEmailCRMCommentActivity);

            TransitionRecord timerTransitionRecord = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = timerActivity,
                SourceActivityRecord = crmCommentPublishRecord,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(timerTransitionRecord);

            TransitionRecord sendEmailCRMCommentTransitionRecord = new TransitionRecord
            {
                WorkflowDefinitionRecord = workflow,
                DestinationActivityRecord = sendEmailCRMCommentActivity,
                SourceActivityRecord = timerActivity,
                DestinationEndpoint = "",
                SourceEndpoint = "Done"
            };
            this.transitionRepository.Create(sendEmailCRMCommentTransitionRecord);

            this.workflowDefinitionRepository.Flush();
            this.activityRepository.Flush();
            this.transitionRepository.Flush();
        }
    }
}