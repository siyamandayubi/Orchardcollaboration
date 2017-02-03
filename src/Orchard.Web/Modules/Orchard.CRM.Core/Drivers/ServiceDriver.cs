using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;

namespace Orchard.CRM.Core.Drivers
{
    public class ServiceDriver : CRMContentPartDriver<ServicePart>
    {
        public ServiceDriver(IOrchardServices services)
            : base(services)
        {
             
        }

        protected override DriverResult Display(
             ServicePart part, string displayType, dynamic shapeHelper)
        {
            string viewName = ServiceDriver.GetViewName(displayType, "Service");
            return ContentShape(viewName,
                () => shapeHelper.Parts_Service(
                    Model: part));
        }

        //GET
        protected override DriverResult Editor(ServicePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Service_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Service",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            ServicePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}