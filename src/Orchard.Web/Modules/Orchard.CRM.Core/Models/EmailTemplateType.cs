using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public enum EmailTemplateType
    {
        TicketCreated = 1,
        TicketClosed = 2,
        NewMessage = 3,
        TicketAssignedToUser = 4,
        FollowersNotification = 5
    }

    public static class EmailTemplateTypeNames
    {
        public const string TicketCreatedName = "Ticket Created";
        public const string TicketClosededName = "Ticket Closed";
        public const string NewMessageName = "New Message";
        public const string TicketAssignedToUserName = "Ticket Assigned to User";
        public const string SendEmailToFollowers = "Followers notification email";
    }
}