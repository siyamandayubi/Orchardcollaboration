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