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
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.Data;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; 

namespace Orchard.CRM.Core.Drivers
{
    public class BusinessUnitMemberDriver : CRMContentPartDriver<BusinessUnitMemberPart>
    {
        protected IRepository<UserPartRecord> userRepository;

        public BusinessUnitMemberDriver(IOrchardServices services, IRepository<UserPartRecord> userRepository)
            : base(services)
        {
            this.userRepository = userRepository;
        }

        protected override DriverResult Display(
             BusinessUnitMemberPart part, string displayType, dynamic shapeHelper)
        {
            string viewName = BusinessUnitMemberDriver.GetViewName(displayType, "BusinessUnitMemberPart");
            return ContentShape(viewName,
                () => shapeHelper.Parts_BusinessUnitMember(
                    Model: part));
        }

        //GET
        protected override DriverResult Editor(BusinessUnitMemberPart part, dynamic shapeHelper)
        {
            int? userId = this.GetPropertyFromRequest("User.Id");

            return ContentShape("Parts_BusinessUnitMember_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/BusinessUnit",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            BusinessUnitMemberPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            int? businessUnitId = this.GetPropertyFromRequest("BusinessUnitMemberPart.BusinessUnitId");

            if (businessUnitId.HasValue)
            {
                part.BusinessUnit = new BusinessUnitPartRecord { Id = businessUnitId.Value };
            }

            int? userId = this.GetPropertyFromRequest("BusinessUnitMemberPart.UserId");

            if (userId.HasValue)
            {
                part.User = this.userRepository.Get(userId.Value);
            }

            return Editor(part, shapeHelper);
        }
    }
}