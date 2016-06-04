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