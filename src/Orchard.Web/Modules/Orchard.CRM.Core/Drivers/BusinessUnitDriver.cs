using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;

namespace Orchard.CRM.Core.Drivers
{
    public class BusinessUnitDriver : CRMContentPartDriver<BusinessUnitPart>
    {
        public BusinessUnitDriver(IOrchardServices services)
            : base(services)
        {
             
        }

        protected override DriverResult Display(
             BusinessUnitPart part, string displayType, dynamic shapeHelper)
        {
            string viewName = BusinessUnitDriver.GetViewName(displayType, "BusinessUnit");
            return ContentShape(viewName,
                () => shapeHelper.Parts_BusinessUnit(
                    Model: part));
        }

        //GET
        protected override DriverResult Editor(BusinessUnitPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_BusinessUnit_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/BusinessUnit",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            BusinessUnitPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}