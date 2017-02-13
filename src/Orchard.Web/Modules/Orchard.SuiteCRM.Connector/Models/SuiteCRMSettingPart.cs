using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Models
{
    public class SuiteCRMSettingPart : ContentPart
    {
        public const string ContentItemTypeName = "SuiteCRMSettings";
        
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

        public int Port
        {
            get { return this.Retrieve(x => x.Port); }
            set { this.Store(x => x.Port, value); }
        }

        public string Database
        {
            get { return this.Retrieve(x => x.Database); }
            set { this.Store(x => x.Database, value); }
        }

        public string Provider
        {
            get { return this.Retrieve(x => x.Provider); }
            set { this.Store(x => x.Provider, value); }
        }

        public string WebAddress
        {
            get { return this.Retrieve(x => x.WebAddress); }
            set { this.Store(x => x.WebAddress, value); }
        }
    }
}