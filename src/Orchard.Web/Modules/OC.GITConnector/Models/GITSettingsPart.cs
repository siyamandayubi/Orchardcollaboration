using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;

namespace OC.GITConnector.Models
{
    public class GITSettingsPart : ContentPart
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

        public string Branches
        {
            get { return this.Retrieve(x => x.Branches); }
            set { this.Store(x => x.Branches, value); }
        }

        public string LocalFolder
        {
            get { return this.Retrieve(x => x.LocalFolder); }
            set { this.Store(x => x.LocalFolder, value); }
        }

        public string RepositoryName
        {
            get { return this.Retrieve(x => x.RepositoryName); }
            set { this.Store(x => x.RepositoryName, value); }
        }
        public string Password
        {
            get { return _password.Value; }
            set { _password.Value = value; }
        }

        public string Server
        {
            get { return this.Retrieve(x => x.Server); }
            set { this.Store(x => x.Server, value); }
        }
        public long LastRevision
        {
            get { return this.Retrieve(x => x.LastRevision); }
            set { this.Store(x => x.LastRevision, value); }
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
        public bool IsValid()
        {
            if (String.IsNullOrWhiteSpace(Server))
            {
                return false;
            }

            return true;
        }
    }
}