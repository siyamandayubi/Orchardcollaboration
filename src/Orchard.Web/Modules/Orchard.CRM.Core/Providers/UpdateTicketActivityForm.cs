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

using NHibernate.Mapping;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Providers
{
    public class UpdateTicketActivityForm : IFormProvider
    {
        private readonly IBasicDataService basicDataService;

        public const string Name = "UpdateTicketActivityForm";
        public const string ServiceId = "ServiceId";
        public const string PriorityId = "PriorityId";
        public const string StatusId = "StatusId";
        public const string DueDateId = "DueDate";
        public const string TicketTitle = "Title";
        public const string TicketDescription = "Description";

        public const string UpdateServiceId = "UpdateServiceId";
        public const string UpdatePriorityId = "UpdatePriorityId";
        public const string UpdateStatusId = "UpdateStatusId";
        public const string UpdateDueDateId = "UpdateDueDate";
        public const string UpdateTicketTitle = "UpdateTitle";

        public UpdateTicketActivityForm(
              IBasicDataService basicDataService,
              IShapeFactory shapeFactory,
              IContentManager contentManager)
        {
            this.basicDataService = basicDataService;
            this.contentManager = contentManager;
            this.Shape = shapeFactory;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        protected IContentManager contentManager;
        protected dynamic Shape { get; set; }

        public void Describe(DescribeContext context)
        {
            Func<IShapeFactory, dynamic> formFactory =
                shapeFactory =>
                {
                    var priorities = this.basicDataService.GetPriorities().ToList();
                    var statusRecords = this.basicDataService.GetStatusRecords().ToList().Select(c => new BasicDataRecordViewModel { Id = c.Id, Name = c.Name }).ToList();
                    var serviceRecords = this.basicDataService.GetServices().ToList();

                    Collection<SelectListItem> prioritySelectList = new Collection<SelectListItem>();
                    Collection<SelectListItem> statusSelectList = new Collection<SelectListItem>();
                    Collection<SelectListItem> serviceSelectList = new Collection<SelectListItem>();

                    Converter.Fill(prioritySelectList, null, priorities);
                    prioritySelectList.Insert(0, new SelectListItem());

                    Converter.Fill(statusSelectList, null, statusRecords);
                    statusSelectList.Insert(0, new SelectListItem());

                    Converter.Fill(serviceSelectList, null, serviceRecords);
                    serviceSelectList.Insert(0, new SelectListItem());

                    Collection<SelectListItem> dueDates = CRMHelper.GetDueDateItems(this.T);

                    var t = this.Shape.Form(
                        Id: "UpdateTicketForm",
                        _Parts: this.Shape.UpdateTicketActivity(
                            Priorities: prioritySelectList,
                            StatusList: statusSelectList,
                            Services: serviceSelectList,
                            DueDates: dueDates
                        ));
  
                    return t;

                };

            context.Form(UpdateTicketActivityForm.Name, formFactory);
        }
    }
}