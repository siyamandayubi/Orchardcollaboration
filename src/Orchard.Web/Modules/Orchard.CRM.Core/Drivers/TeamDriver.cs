using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.Data;

namespace Orchard.CRM.Core.Drivers
{
    public class TeamDriver : CRMContentPartDriver<TeamPart>
    {
        protected IRepository<BusinessUnitPartRecord> businessUnitRepository;

        public TeamDriver(IOrchardServices services, IRepository<BusinessUnitPartRecord> businessUnitRepository)
            : base(services)
        {
            this.businessUnitRepository = businessUnitRepository;
            this.services = services;
        }

        protected override DriverResult Display(
             TeamPart part, string displayType, dynamic shapeHelper)
        {
            string viewName = TeamDriver.GetViewName(displayType, "Team");
            return ContentShape(viewName,
                () => shapeHelper.Parts_Team(
                    Model: part));
        }

        //GET
        protected override DriverResult Editor(TeamPart part, dynamic shapeHelper)
        {
            int? businessUnitId = this.GetPropertyFromRequest("parentId");

            if (businessUnitId != null)
            {
                part.BusinessUnit = this.businessUnitRepository.Get(businessUnitId.Value);
            }

            return ContentShape("Parts_Team_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Team",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            TeamPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            int? businessUnitId = this.GetPropertyFromRequest("BusinessUnit.Id");

            if (businessUnitId.HasValue)
            {
                part.BusinessUnit = new Models.BusinessUnitPartRecord { Id = businessUnitId.Value };
            }

            return Editor(part, shapeHelper);
        }
    }
}