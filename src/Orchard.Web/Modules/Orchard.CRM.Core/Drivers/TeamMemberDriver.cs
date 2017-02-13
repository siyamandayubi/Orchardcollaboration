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
    public class TeamMemberDriver : CRMContentPartDriver<TeamMemberPart>
    {
        protected IRepository<UserPartRecord> userRepository;

        public TeamMemberDriver(IOrchardServices services, IRepository<UserPartRecord> userRepository)
            : base(services)
        {
            this.userRepository = userRepository;
        }

        protected override DriverResult Display(
             TeamMemberPart part, string displayType, dynamic shapeHelper)
        {
            string viewName = TeamMemberDriver.GetViewName(displayType, "TeamMemberPart");
            return ContentShape(viewName,
                () => shapeHelper.Parts_TeamMember(
                    Model: part));
        }

        //GET
        protected override DriverResult Editor(TeamMemberPart part, dynamic shapeHelper)
        {
            int? userId = this.GetPropertyFromRequest("User.Id");

            return ContentShape("Parts_TeamMember_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Team",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            TeamMemberPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            int? teamId = this.GetPropertyFromRequest("TeamMemberPart.TeamId");

            if (teamId.HasValue)
            {
                part.Team = new TeamPartRecord { Id = teamId.Value };
            }

            int? userId = this.GetPropertyFromRequest("TeamMemberPart.UserId");

            if (userId.HasValue)
            {
                part.User = this.userRepository.Get(userId.Value);
            }

            return Editor(part, shapeHelper);
        }
    }
}