using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;
using Orchard.Email.Activities;
using Orchard.Email.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Messaging.Services;
using Orchard.Users.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Activities
{
    public abstract class CRMSendEmailActivity : Task
    {
        private readonly IMessageService messageService;
        private readonly IJobsQueueService jobsQueueService;
        private readonly IRepository<BusinessUnitMemberPartRecord> businessUnitMembersRepository;
        private readonly IRepository<TeamMemberPartRecord> teamMembersRepository;

        protected readonly IRepository<UserPartRecord> userRepository;

        public CRMSendEmailActivity(
            IMessageService messageService,
            IJobsQueueService jobsQueueService,
            IRepository<BusinessUnitMemberPartRecord> businessUnitMembersRepository,
            IRepository<TeamMemberPartRecord> teamMembersRepository,
            IRepository<UserPartRecord> userRepository)
        {
            this.messageService = messageService;
            this.jobsQueueService = jobsQueueService;
            this.userRepository = userRepository;
            this.businessUnitMembersRepository = businessUnitMembersRepository;
            this.teamMembersRepository = teamMembersRepository;
        }

        public IEnumerable<UserPartRecord> GetRecipients(ContentItemPermissionDetailRecord permission)
        {
            List<UserPartRecord> recipients = new List<UserPartRecord>();
            if (permission.BusinessUnit != null)
            {
                var members = this.businessUnitMembersRepository.Table.Where(c => c.BusinessUnitPartRecord.Id == permission.BusinessUnit.Id).ToList();
                foreach (var member in members.Where(c => c.UserPartRecord != null && !string.IsNullOrEmpty(c.UserPartRecord.Email)))
                {
                    recipients.Add(member.UserPartRecord);
                }
            }
            else if (permission.Team != null)
            {
                var members = this.teamMembersRepository.Table.Where(c => c.TeamPartRecord.Id == permission.Team.Id);
                foreach (var member in members.Where(c => c.UserPartRecord != null && !string.IsNullOrEmpty(c.UserPartRecord.Email)))
                {
                    recipients.Add(member.UserPartRecord);
                }
            }
            else if (permission.User != null)
            {
                var user = this.userRepository.Table.FirstOrDefault(c => c.Id == permission.User.Id);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    recipients.Add(user);
                }
            }

            return recipients;
        }

        protected void SendEmail(string subject, string body, string recipients, bool queued, int priority)
        {
            var parameters = new Dictionary<string, object> {
                {"Subject", subject},
                {"Body", body},
                {"Recipients", recipients}
            };

            if (!queued)
            {
                this.messageService.Send(CRMHelper.OrchardCollaborationEmailMessageType, parameters);
            }
            else
            {
                this.jobsQueueService.Enqueue("IMessageService.Send", new { type = CRMHelper.OrchardCollaborationEmailMessageType, parameters = parameters }, priority);
            }

        }
    }
}