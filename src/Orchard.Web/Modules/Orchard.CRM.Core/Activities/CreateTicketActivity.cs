namespace Orchard.CRM.Core.Activities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.MetaData;
    using Orchard.Core.Common.Models;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using Orchard.CRM.Core.Services;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Logging;
    using Orchard.Tokens;
    using Orchard.Users.Models;
    using Orchard.Workflows.Models;
    using Orchard.Workflows.Services;
    using S22.IMAP.Models;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;
    using System.Linq;

    public class CreateTicketActivity : Task
    {
        public const string ActivityName = "CreateTicket";

        private readonly IContentManager contentManager;
        private readonly IFileService fileService;
        private readonly IRepository<ContentItemPermissionDetailRecord> contentItemPermissionDetailRepository;
        private readonly IBasicDataService basicDataService;
        protected readonly ICRMContentOwnershipService contentOwnershipService;
        protected readonly IContentOwnershipHelper contentOwnershipHelper;
        protected readonly IRepository<TicketIdentityRecord> ticketIdentityRecordRepository;
        private readonly ITokenizer tokenizer;
        private readonly IContentDefinitionManager contentDefinitionManager;
        private readonly IActivityStreamService activityStreamService;
        private readonly IRepository<UserPartRecord> userRepository;

        public CreateTicketActivity(
            IRepository<UserPartRecord> userRepository,
            IActivityStreamService activityStreamService,
            IFileService fileService,
            ITokenizer tokenizer,
            IContentOwnershipHelper contentOwnershipHelper,
            ICRMContentOwnershipService contentOwnershipService,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IBasicDataService basicDataService,
            IRepository<TicketIdentityRecord> ticketIdentityRecordRepository,
            IRepository<ContentItemPermissionDetailRecord> contentItemPermissionDetailRepository)
        {
            this.userRepository = userRepository;
            this.fileService = fileService;
            this.contentDefinitionManager = contentDefinitionManager;
            this.contentOwnershipHelper = contentOwnershipHelper;
            this.tokenizer = tokenizer;
            this.ticketIdentityRecordRepository = ticketIdentityRecordRepository;
            this.contentOwnershipService = contentOwnershipService;
            this.basicDataService = basicDataService;
            this.contentItemPermissionDetailRepository = contentItemPermissionDetailRepository;
            this.contentManager = contentManager;
            this.activityStreamService = activityStreamService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public override string Name
        {
            get { return ActivityName; }
        }

        public override string Form
        {
            get
            {
                return CreateTicketActivityForm.Name;
            }
        }
        public override LocalizedString Category
        {
            get { return T("CRM Core"); }
        }

        public override LocalizedString Description
        {
            get { return T("Create a new Ticket"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            ContentItem contentItem = workflowContext.Content.ContentItem;

            var ticket = this.contentManager.New("Ticket");
            this.contentManager.Create(ticket, VersionOptions.Draft);
            var ticketPart = ticket.As<TicketPart>();

            var parentTicket = contentItem.As<TicketPart>();
            if (parentTicket != null)
            {
                ticketPart.Record.Parent = parentTicket.Record;
            }

            // requesting user
            if (parentTicket != null)
            {
                ticketPart.Record.RequestingUser = parentTicket.Record.RequestingUser;
            }
            else if (contentItem != null)
            {
                var commonPart = contentItem.As<CommonPart>();
                if (commonPart != null && commonPart.Record.OwnerId != default(int))
                {
                    ticketPart.Record.RequestingUser = new UserPartRecord { Id = commonPart.Record.OwnerId };
                }
            }

            // service
            int? serviceId = GetValueFromActivityContext(activityContext, CreateTicketActivityForm.ServiceId);
            if (serviceId.HasValue)
            {
                ticketPart.Record.Service = new ServicePartRecord { Id = serviceId.Value };
            }

            // status
            int? statusId = GetValueFromActivityContext(activityContext, CreateTicketActivityForm.StatusId);
            if (statusId.HasValue)
            {
                ticketPart.Record.StatusRecord = new StatusRecord { Id = statusId.Value };
            }

            // Project
            AttachToProjectPart attachToProjectPart = ticket.As<AttachToProjectPart>();
            if (attachToProjectPart != null)
            {
                string valueString = activityContext.GetState<string>(CreateTicketActivityForm.ProjectId);

                // check wether the value is id or identity                
                int projectId;

                if (!int.TryParse(valueString, out projectId))
                {
                    var project = this.contentManager.Query<IdentityPart>().Where<IdentityPartRecord>(c => c.Identifier == valueString).Slice(0, 1).FirstOrDefault();
                    if (project != null)
                    {
                        projectId = project.ContentItem.Id;
                    }
                }
                if (projectId != default(int))
                {
                    attachToProjectPart.Record.Project = new ProjectPartRecord { Id = projectId };
                }
            }

            // priority
            int? priorityId = GetValueFromActivityContext(activityContext, CreateTicketActivityForm.PriorityId);
            if (priorityId.HasValue)
            {
                ticketPart.Record.PriorityRecord = new PriorityRecord { Id = priorityId.Value };
            }

            // Due date
            int? dueDateDays = GetValueFromActivityContext(activityContext, CreateTicketActivityForm.DueDateId);
            if (dueDateDays.HasValue)
            {
                ticketPart.Record.DueDate = DateTime.UtcNow.AddDays(dueDateDays.Value);
            }

            // Title
            string title = activityContext.GetState<string>(CreateTicketActivityForm.TicketTitle);
            if (!string.IsNullOrEmpty(title))
            {
                title = title.Length > 100 ?
                    title.Substring(0, 100) :
                    title;
            }
            ticketPart.Record.Title = title;

            // Description
            string description = activityContext.GetState<string>(CreateTicketActivityForm.TicketDescription);
            if (!string.IsNullOrEmpty(description))
            {
                description = description.Length > 3000 ?
                    description.Substring(0, 3000) :
                    description;
            }

            ticketPart.Record.Description = description;

            // Received email is not a RelatedContentItem
            if (contentItem.ContentType != IMAPEmailPart.ContentItemTypeName)
            {
                ticketPart.Record.RelatedContentItem = workflowContext.Content.ContentItem.Record;
            }

            // Identity
            var identityRecord = new TicketIdentityRecord();
            this.ticketIdentityRecordRepository.Create(identityRecord);
            ticketPart.Record.Identity = identityRecord;

            // Permission
            this.SetPermissions(workflowContext, activityContext, ticket, ticketPart);

            this.AddEmailAttachmentAndRequestingUser(workflowContext, activityContext, ticket);

            this.contentManager.Publish(ticket);
            this.activityStreamService.WriteChangesToStreamActivity(ticket, null, true, StreamWriters.TicketStreamWriter);

            return new[] { T("Done") };
        }

        private int? GetValueFromActivityContext(ActivityContext activityContext, string key)
        {
            string valueString = activityContext.GetState<string>(key);
            int value;
            return int.TryParse(valueString, out value) ? (int?)value : null;
        }

        private void SetPermissions(WorkflowContext workflowContext, ActivityContext activityContext, ContentItem ticket, TicketPart ticketPart)
        {
            // Permission
            var contentPermissionPart = ticket.As<ContentItemPermissionPart>();
            contentPermissionPart.Record.Ticket = ticketPart.Record;

            // EditableBy
            string editableBy = activityContext.GetState<string>("EditableBy");
            List<UserPartRecord> users = new List<UserPartRecord>();
            if (!string.IsNullOrEmpty(editableBy))
            {
                var usernames = editableBy.Split(',').Select(c => c.Trim().ToLower()).ToArray();
                users = this.userRepository.Table.Where(c => usernames.Contains(c.UserName)).ToList();

                // remove redundants
                users = users.GroupBy(c => c.Id).Select(c => c.First()).ToList();
                foreach (var user in users)
                {
                    var permissionRecord = new ContentItemPermissionDetailRecord
                    {
                        AccessType = ContentItemPermissionAccessTypes.SharedForEdit,
                        User = new UserPartRecord { Id = user.Id },
                        ContentItemPermissionPartRecord = contentPermissionPart.Record
                    };
                    this.contentOwnershipHelper.Create(permissionRecord, ticket, false);
                }
            }

            // businessUnit
            string groupId = activityContext.GetState<string>("GroupId");
            if (!string.IsNullOrEmpty(groupId))
            {
                var targetContentItemPermissionViewModel = Converter.DecodeGroupId(groupId);

                if (targetContentItemPermissionViewModel.BusinessUnitId.HasValue)
                {
                    var permissionRecord = new ContentItemPermissionDetailRecord
                    {
                        AccessType = ContentItemPermissionAccessTypes.Assignee,
                        BusinessUnit = new BusinessUnitPartRecord { Id = targetContentItemPermissionViewModel.BusinessUnitId.Value },
                        ContentItemPermissionPartRecord = contentPermissionPart.Record
                    };

                    this.contentOwnershipHelper.Create(permissionRecord, ticket, false);
                    contentPermissionPart.Record.HasOwner = true;
                }
                else if (targetContentItemPermissionViewModel.TeamId.HasValue)
                {
                    var permissionRecord = new ContentItemPermissionDetailRecord
                    {
                        AccessType = ContentItemPermissionAccessTypes.Assignee,
                        Team = new TeamPartRecord { Id = targetContentItemPermissionViewModel.TeamId.Value },
                        ContentItemPermissionPartRecord = contentPermissionPart.Record
                    };

                    this.contentOwnershipHelper.Create(permissionRecord, ticket, false);
                    contentPermissionPart.Record.HasOwner = true;
                }
            }
            else
            {
                // user
                int? userId = GetValueFromActivityContext(activityContext, CreateTicketActivityForm.SelectedUserId);
                if (userId != null && !users.Any(c => c.Id == userId.Value))
                {
                    var permissionRecord = new ContentItemPermissionDetailRecord
                    {
                        AccessType = ContentItemPermissionAccessTypes.Assignee,
                        User = new UserPartRecord { Id = userId.Value },
                        ContentItemPermissionPartRecord = contentPermissionPart.Record
                    };

                    contentPermissionPart.Record.HasOwner = true;
                    this.contentOwnershipHelper.Create(permissionRecord, ticket, false);
                }
            }
        }

        private void AddEmailAttachmentAndRequestingUser(WorkflowContext workflowContext, ActivityContext activityContext, ContentItem ticket)
        {
            ContentItem contentItem = workflowContext.Content.ContentItem;
            var emailPart = contentItem.As<IMAPEmailPart>();

            if (emailPart == null)
            {
                return;
            }

            // try to get user
            dynamic sender = JObject.Parse(emailPart.From);
            string email = sender.Email;
            var user = this.basicDataService.GetOperatorOrCustomerUser(email);
            bool ignoreUnknownEmail = activityContext.GetState<bool>("IgnoreUnknownEmail");

            // if there is no user and the email is blocked, then do noting
            if (ignoreUnknownEmail && user == null)
            {
                return;
            }

            var ticketPart = ticket.As<TicketPart>();
            ticketPart.Record.SourceId = TicketSourceTypes.Email;

            // cut SourceData if it is too long
            string source = emailPart.From;
            if (!string.IsNullOrEmpty(source))
            {
                source = source.Length > 100 ? source.Substring(0, 100) : source;
            }
            ticketPart.Record.SourceData = source;

            if (user != null)
            {
                ticketPart.Record.RequestingUser = new UserPartRecord { Id = user.Id };
            }
            else
            {
                string from = emailPart.MailMessage != null ? emailPart.MailMessage.From.ToString() : emailPart.From;
                string temp = string.Format(CultureInfo.InvariantCulture, "Sent by {0}<hr></hr>", HttpUtility.HtmlEncode(from));
                ticketPart.Description = temp + ticketPart.Description;
            }

            // download attachments
            bool downloadAttachments = activityContext.GetState<bool>("DownloadEmailAttachments");
            var fileUploadPart = ticket.As<FileUploadPart>();
            if (downloadAttachments && emailPart.MailMessage != null && fileUploadPart != null)
            {
                // set guid
                if (fileUploadPart.Record.FolderGuid == Guid.Empty)
                {
                    fileUploadPart.Record.FolderGuid = Guid.NewGuid();
                }

                Dictionary<string, string> errors = new Dictionary<string, string>();
                foreach (var attachment in emailPart.MailMessage.Attachments)
                {
                    this.fileService.AddFile(attachment.Name, attachment.ContentStream, ticket.ContentType, fileUploadPart.Record.FolderGuid, errors);
                }
            }

            // cut title if it is too long
            if (!string.IsNullOrEmpty(ticketPart.Record.Title))
            {
                ticketPart.Record.Title = ticketPart.Record.Title.Length > 100 ? ticketPart.Record.Title.Substring(0, 100) : ticketPart.Record.Title;
            }
        }
    }
}