using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace S22.IMAP.Models
{
    public class IMAPSettingPart : ContentPart
    {
        private readonly ComputedField<string> _password = new ComputedField<string>();

        public ComputedField<string> PasswordField
        {
            get { return _password; }
        }

        public string Username
        {
            get { return this.Retrieve(x => x.Username); }
            set { this.Store(x => x.Username, value); }
        }

        public string Password
        {
            get { return _password.Value; }
            set { _password.Value = value; }
        }
        
        public string Host
        {
            get { return this.Retrieve(x => x.Host); }
            set { this.Store(x => x.Host, value); }
        }

        public bool EnableSsl
        {
            get { return this.Retrieve(x => x.EnableSsl); }
            set { this.Store(x => x.EnableSsl, value); }
        }

        public string LatestError
        {
            get { return this.Retrieve(x => x.LatestError, string.Empty); }
            set { this.Store(x => x.LatestError, value); }
        }

        public DateTime? LatestErrorTime
        {
            get { return this.Retrieve(x => x.LatestErrorTime, (DateTime?)null); }
            set { this.Store(x => x.LatestErrorTime, value); }
        }

        public DateTime? LastSuccessfullConnectionTime
        {
            get { return this.Retrieve(x => x.LastSuccessfullConnectionTime, (DateTime?)null); }
            set { this.Store(x => x.LastSuccessfullConnectionTime, value); }
        }

        public int Port
        {
            get { return this.Retrieve(x => x.Port, 993); }
            set { this.Store(x => x.Port, value); }
        }

        public string DefaultBox
        {
            get { return this.Retrieve(x => x.DefaultBox, "INBOX"); }
            set { this.Store(x => x.DefaultBox, value); }
        }

        public int EmailsFromMinutesBefore
        {
            get { return this.Retrieve(x => x.EmailsFromMinutesBefore, 0); }
            set { this.Store(x => x.EmailsFromMinutesBefore, value); }
        }
        public bool IsValid()
        {
            if (String.IsNullOrWhiteSpace(Host))
            {
                return false;
            }

            if (!String.IsNullOrWhiteSpace(Host) && Port == 0)
            {
                return false;
            }

            return true;
        }
    }
}