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
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Activities;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Providers.ActivityStream;
using Orchard.Data;
using Orchard.DisplayManagement.Shapes;
using Orchard.Email.Activities;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Messaging.Services;
using Orchard.Security;
using Orchard.Tokens;
using Orchard.Users.Models;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Activities
{
    public class SendEmailToFollowersActivity : CRMSendEmailActivity
    {
        private readonly ITokenizer tokenizer;
        private readonly IOrchardServices services;
        private IRepository<EmailTemplateRecord> emailTemplateRepository;
        private IRepository<ActivityStreamRecord> activityStreamRepository;

        public SendEmailToFollowersActivity(
               IOrchardServices services,
               IMessageService messageService,
               IJobsQueueService jobsQueueService,
               IRepository<EmailTemplateRecord> emailTemplateRepository,
               ITokenizer tokenizer,
               IRepository<BusinessUnitMemberPartRecord> businessUnitMembersRepository,
               IRepository<ActivityStreamRecord> activityStreamRepository,
               IRepository<TeamMemberPartRecord> teamMembersRepository,
               IRepository<UserPartRecord> userRepository)
            : base(messageService, jobsQueueService, businessUnitMembersRepository, teamMembersRepository, userRepository)
        {
            this.activityStreamRepository = activityStreamRepository;
            this.services = services;
            this.tokenizer = tokenizer;
            this.emailTemplateRepository = emailTemplateRepository;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public override string Form
        {
            get
            {
                return EmailTemplateActivityForm.Name;
            }
        }

        public override string Name
        {
            get { return "SendEmailToFollowersActivity"; }
        }

        public override LocalizedString Category
        {
            get { return T("CRM Project"); }
        }

        public override LocalizedString Description
        {
            get { return T("Send email to the followers"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done"), T("Failed") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            FollowerPart followerPart = workflowContext.Content.As<FollowerPart>();
            if (followerPart == null)
            {
                this.Logger.Debug("ContentItem has no FollowerPart");
                return new[] { T("Failed") };
            }

            // setup tokenizer
            Dictionary<string, object> temp = new Dictionary<string, object>();
            temp.Add("Content", workflowContext.Content);

            CRMCommentPart commentPart = workflowContext.Content.As<CRMCommentPart>();
            if (commentPart != null)
            {
                temp.Add(ActivityTokenProvider.CRMCommentKey, commentPart);
            }

            string emailTemplateIdString = activityContext.GetState<string>(EmailTemplateActivityForm.EmailTemplateIdFieldName);
            int emailTemplateId;
            if (!int.TryParse(emailTemplateIdString, out emailTemplateId))
            {
                this.Logger.Debug("There is no email Template");
                return new[] { T("Failed") };
            }

            var ticketEmailTemplate = this.emailTemplateRepository.Table.First(c => c.Id == emailTemplateId);

            if (ticketEmailTemplate == null)
            {
                this.Logger.Debug("There is no email Template");
                return new[] { T("Failed") };
            }

            var queued = activityContext.GetState<bool>("Queued");
            var priority = activityContext.GetState<int>("Priority");

            List<int> userIds = string.IsNullOrEmpty(followerPart.Followers) ? null : followerPart.Followers.Split(',').Select(c => int.Parse(c)).ToList();

            if (userIds == null)
            {
                return new[] { T("Done") };
            }

            // Add activity stream record
            int? activityStreamRecordId = workflowContext.GetState<int?>("ActivityStreamId");

            ActivityStreamRecord activityStreamRecord = null;
            if (activityStreamRecordId != null)
            {
                activityStreamRecord = this.activityStreamRepository.Table.First(c => c.Id == activityStreamRecordId.Value);

                temp.Add(ActivityStreamTokenProvider.ActivityStreamRecordKey, activityStreamRecord);

                // do not send notification to himself/herself
                if (activityStreamRecord.User != null)
                {
                    userIds.Remove(activityStreamRecord.User.Id);
                }
            }
            else
            {
                return new[] { T("Failed") };
            }
            
            var recipients = this.services.ContentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);
            foreach (var recipient in recipients.Where(c => !string.IsNullOrEmpty(c.Email)))
            {
                var userPart = recipient.As<UserPart>();

                if (userPart != null)
                {
                    temp.Add(ActivityTokenProvider.UserKey, userPart.Record);
                }

                string body = this.tokenizer.Replace(ticketEmailTemplate.Body, temp);
                string subject = this.tokenizer.Replace(ticketEmailTemplate.Subject, temp);
                this.SendEmail(subject, body, recipient.Email, queued, priority);
            }

            return new[] { T("Done") };
        }
    }
}