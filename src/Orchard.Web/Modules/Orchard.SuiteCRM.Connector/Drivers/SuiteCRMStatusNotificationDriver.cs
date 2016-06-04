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

using Orchard.ContentManagement.Drivers;
using Orchard.Logging;
using Orchard.SuiteCRM.Connector.Models;
using Orchard.SuiteCRM.Connector.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Drivers
{
    public class SuiteCRMStatusNotificationDriver : ContentPartDriver<SuiteCRMStatusNotificationPart>
    {
        private readonly IOrchardServices services;

        public SuiteCRMStatusNotificationDriver(IOrchardServices services)
        {
            this.services = services;
            Logger = NullLogger.Instance;
        }
       
        public ILogger Logger { get; set; }

        protected override DriverResult Display(SuiteCRMStatusNotificationPart part, string displayType, dynamic shapeHelper)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageSuiteCRMPermission))
            {
                return null;
            }

            bool isConnectionAvailable = false;
            try
            {
                using (var connection = Helper.GetConnection(this.services, this.Logger))
                {
                    connection.Open();
                    isConnectionAvailable = connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                isConnectionAvailable = false;
            }

            if (isConnectionAvailable)
            {
                return null;
            }

            return ContentShape("Parts_SuiteCRMStatusNotification", () => shapeHelper.Parts_SuiteCRMStatusNotification(Model: part));
        }
    }
}