using OC.SVNConnector.Models;
using OC.SVNConnector.Tickets.Models;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

namespace OC.SVNConnector.Tickets.Activities
{
    public class AttachSVNToTicketActivity : Task
    {
        public const string ActivityName = "ConnectSVNLogToTicket";

        private readonly IWorkflowManager workflowManager;
        private readonly ISearchTicketService ticketSearchService;
        public AttachSVNToTicketActivity(
            ISearchTicketService ticketSearchService,
            IWorkflowManager workflowManager)
        {
            this.workflowManager = workflowManager;
            this.ticketSearchService = ticketSearchService;
        }

        public Localizer T { get; set; }

        public override string Name
        {
            get { return ActivityName; }
        }

        public override LocalizedString Description
        {
            get { return T("Connect SVN Log to tickets based on the ticket number. If log message contains #{TicketNumber}, then the activity connects the log to the ticket"); }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done"), T("NotSVNLogItem"), T("LogHasNoTicketNumber"), T("TicketNotFound") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            ContentItem contentItem = workflowContext.Content.ContentItem;

            if (contentItem == null)
            {
                return new[] { T("NotSVNLogItem") };
            }

            SVNLogPart svnLogPart = contentItem.As<SVNLogPart>();
            if (svnLogPart == null)
            {
                return new[] { T("NotSVNLogItem") };
            }

            if (string.IsNullOrEmpty(svnLogPart.LogMessage))
            {
                return new[] { T("LogHasNoTicketNumber") };
            }

            var message = svnLogPart.LogMessage;
            var matches = Regex.Matches(message, @"([#]\d+)");
            List<int> ticketNumbers = new List<int>();
            foreach (Match match in matches)
            {
                string number = message.Substring(match.Index + 1, match.Length - 1);
                int ticketNumber;
                if (int.TryParse(number, out ticketNumber))
                {
                    ticketNumbers.Add(ticketNumber);
                }
            }

            if (ticketNumbers.Count == 0)
            {
                return new[] { T("LogHasNoTicketNumber") };
            }

            List<TicketPart> tickets = new List<TicketPart>();

            foreach (var id in ticketNumbers)
            {
                var ticket = ticketSearchService.GetByTicketNumber(id);
                if (ticket != null)
                {
                    tickets.Add(ticket);
                }
            }

            if(tickets.Count == 0)
            {
                return new[] { T("LogHasNoTicketNumber") };
            }

            ticketNumbers = ticketNumbers.Distinct().ToList();

            foreach (var ticket in tickets)
            {
                SVNLogsTicketPart svnLogsTicketPart = ticket.As<SVNLogsTicketPart>();
                if(svnLogsTicketPart != null)
                {
                    if (string.IsNullOrEmpty(svnLogsTicketPart.SVNLogs))
                    {
                        svnLogsTicketPart.SVNLogs = svnLogPart.Id.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        svnLogsTicketPart.SVNLogs += "," + svnLogPart.Id.ToString(CultureInfo.InvariantCulture);
                    }
                }

                workflowManager.TriggerEvent(
                    SVNLogConnectedToTicketActivity.ActivityName, ticket.ContentItem,
                    () => new Dictionary<string, object> {
                        { "Content", ticket.ContentItem },
                        { "SVNLog", svnLogPart } });
            }

            return new[] { T("Done") };
        }

        public override LocalizedString Category
        {
            get { return T("SVN"); }
        }
    }
}