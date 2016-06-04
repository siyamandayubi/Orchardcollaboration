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
using Orchard.CRM.Core.Services;
using Orchard.SuiteCRM.Connector.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.SuiteCRM.Connector.Services;
using Orchard.CRM.Core.Models;
using Orchard.Security;

namespace Orchard.SuiteCRM.Connector.Drivers
{
    public class SuiteCRMTaskDriver : ContentPartDriver<SuiteCRMTaskPart>
    {
        private readonly IOrchardServices services;
        private readonly IWorkContextAccessor workContextAccessor;
        private readonly ISuiteCRMDataService suiteCRMDataService;
        protected readonly ICRMContentOwnershipService contentOwnershipService;

        public SuiteCRMTaskDriver(
            ISuiteCRMDataService suiteCRMDataService,
            IOrchardServices services,
            IWorkContextAccessor workContextAccessor,
            ICRMContentOwnershipService contentOwnershipService)
        {
            this.suiteCRMDataService = suiteCRMDataService;
            this.contentOwnershipService = contentOwnershipService;
            this.workContextAccessor = workContextAccessor;
            this.services = services;
        }

        protected override DriverResult Display(SuiteCRMTaskPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType != "Detail")
            {
                return null;
            }

            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return null;
            }

            var ticket = part.As<TicketPart>();
            if (ticket == null || ticket.Record.RequestingUser == null)
            {
                return null;
            }

            var user = this.services.ContentManager.Get<IUser>(ticket.Record.RequestingUser.Id);

            return ContentShape("Parts_SuiteCRMTaskContactLink",
                 () => shapeHelper.Parts_SuiteCRMTaskContactLink(
                     Model: user
                     ));
        }

        protected DriverResult DisplaySyncOptions(SuiteCRMTaskPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType != "Detail")
            {
                return null;
            }

            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return null;
            }

            var setting = services.WorkContext.CurrentSite.As<SuiteCRMSettingPart>();

            if (setting == null)
            {
                return null;
            }

            string taskType = part.TaskType ?? SuiteCRMTaskPart.SuiteCRMProjectTaskTypeValue;
            var attachToProjectPart = part.As<AttachToProjectPart>();
            dynamic model = new ExpandoObject();
            model.LastSyncTime = part.LastSyncTime;
            model.ExternalId = part.ExternalId;
            model.TaskId = part.Id;
            model.IsProjectTask = attachToProjectPart != null && attachToProjectPart.Record.Project != null;
            model.SuiteCRMAddress = (!string.IsNullOrEmpty(setting.WebAddress) && !string.IsNullOrEmpty(part.ExternalId)) ?
                Helper.GetTaskAddressInSuiteCRM(this.services, part.ExternalId, taskType) :
                string.Empty;

            var context = this.workContextAccessor.GetContext();
            Helper.RenderSyncDialogs(context, shapeHelper, model);

            TicketPart ticketPart = part.As<TicketPart>();
            if (displayType == "Detail" && ticketPart.Record.RequestingUser != null)
            {
                var reporterUser = this.services.ContentManager.Get<IUser>(ticketPart.Record.RequestingUser.Id);
                var contact = this.suiteCRMDataService.GetContact(reporterUser.Email);
                if (contact != null)
                {
                    model.ContactUrl = Helper.GetContactAddressInSuiteCRM(this.services, contact.bean_id);
                }
            }

            return ContentShape("Parts_SuiteCRMTask",
                 () => shapeHelper.Parts_SuiteCRMTask(
                     Model: model
                     ));
        }
        protected override DriverResult Editor(SuiteCRMTaskPart part, ContentManagement.IUpdateModel updater, dynamic shapeHelper)
        {
            return null;
        }

        protected override DriverResult Editor(SuiteCRMTaskPart part, dynamic shapeHelper)
        {
            return null;
        }
    }
}