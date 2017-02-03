using Orchard.ContentManagement;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class TeamMemberPart : ContentPart<TeamMemberPartRecord>
    {
        public TeamPartRecord Team
        {
            get
            {
                return this.Record.TeamPartRecord;
            }
            set
            {
                this.Record.TeamPartRecord = value;
            }
        }

        public UserPartRecord User
        {
            get
            {
                return this.Record.UserPartRecord;
            }
            set
            {
                this.Record.UserPartRecord = value;
            }
        }
    }
}