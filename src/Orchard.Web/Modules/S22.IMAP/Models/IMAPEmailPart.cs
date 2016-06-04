using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace S22.IMAP.Models
{
    public class IMAPEmailPart : ContentPart
    {
        public const string ContentItemTypeName = "ReceivedEmail";
        public string From
        {
            get { return this.Retrieve(x => x.From, string.Empty); }
            set { this.Store(x => x.From, value); }
        }

        public string Subject
        {
            get { return this.Retrieve(x => x.Subject, string.Empty); }
            set { this.Store(x => x.Subject, value); }
        }

        public long UId
        {
            get { return this.Retrieve(x => x.UId, default(long)); }
            set { this.Store(x => x.UId, value); }
        }

        /// <summary>
        /// It is temprorary property. We don't persist MailMessage for security reasons
        /// </summary>
        public MailMessage MailMessage { get; set; }
    }
}