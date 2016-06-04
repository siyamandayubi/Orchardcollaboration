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
using Orchard.SuiteCRM.Connector.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Orchard.ContentManagement;
using System.Web;
using Orchard.SuiteCRM.Connector.Services;
using Orchard.CRM.Core.Services;

namespace Orchard.SuiteCRM.Connector.Drivers
{
    public class SuiteCRMProjectDriver : ContentPartDriver<SuiteCRMProjectPart>
    {
        private readonly IOrchardServices services;
        private readonly IWorkContextAccessor workContextAccessor;
        protected readonly ICRMContentOwnershipService contentOwnershipService;

        public SuiteCRMProjectDriver(
            IOrchardServices services, 
            IWorkContextAccessor workContextAccessor,
            ICRMContentOwnershipService contentOwnershipService)
        {
            this.contentOwnershipService = contentOwnershipService;
            this.workContextAccessor = workContextAccessor;
            this.services = services;
        }

        protected override DriverResult Display(SuiteCRMProjectPart part, string displayType, dynamic shapeHelper)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return null;
            }

            var setting = services.WorkContext.CurrentSite.As<SuiteCRMSettingPart>();

            if (setting == null)
            {
                return null;
            }

            dynamic model = new ExpandoObject();
            model.LastSyncTime = part.LastSyncTime;
            model.ExternalId = part.ExternalId;
            model.ProjectId = part.Id;
            model.SuiteCRMAddress = (!string.IsNullOrEmpty(setting.WebAddress) && !string.IsNullOrEmpty(part.ExternalId)) ?
                Helper.GetProjectAddressInSuiteCRM(this.services, part.ExternalId) :
                string.Empty;

            var context = this.workContextAccessor.GetContext();
            Helper.RenderSyncDialogs(context, shapeHelper, model);

            return ContentShape("Parts_SuiteCRMProject",
                 () => shapeHelper.Parts_SuiteCRMProject(
                     Model: model
                     ));
        }

        protected override DriverResult Editor(SuiteCRMProjectPart part, ContentManagement.IUpdateModel updater, dynamic shapeHelper)
        {
            return null;
        }

        protected override DriverResult Editor(SuiteCRMProjectPart part, dynamic shapeHelper)
        {
            return null;
        }
    }
}