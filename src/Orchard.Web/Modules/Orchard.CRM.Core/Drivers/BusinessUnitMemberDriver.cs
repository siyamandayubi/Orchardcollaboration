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