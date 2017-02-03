using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class TeamPart : ContentPart<TeamPartRecord>
    {
        public string Name
        {
            get { return this.Record.Name; }
            set { this.Record.Name = value; }
        }

        public virtual BusinessUnitPartRecord BusinessUnit
        {
            get
            {
                return this.Record.BusinessUnitPartRecord;
            }
            set
            {
                this.Record.BusinessUnitPartRecord = value;
            }
        }

        public virtual IList<TeamMemberPartRecord> TeamMembers
        {
            get
            {
                return this.Record.TeamMembers;
            }
        }


        public virtual string Description
        {
            get
            {
                return this.Record.Description;
            }
            set
            {
                this.Record.Description = value;
            }
        }
    }
}