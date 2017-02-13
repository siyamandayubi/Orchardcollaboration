using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class BusinessUnitPart : ContentPart<BusinessUnitPartRecord>
    {
        public string Name
        {
            get { return this.Record.Name; }
            set { this.Record.Name = value; }
        }

        public string Description
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

        public BusinessUnitPartRecord Parent
        {
            get
            {
                return this.Record.Parent;
            }
            set
            {
                this.Record.Parent = value;
            }
        }

        public IList<TeamPartRecord> Teams
        {
            get
            {
                return this.Record.Teams;
            }
        }

        public IList<BusinessUnitMemberPartRecord> Members
        {
            get
            {
                return this.Record.Members;
            }
        }

        public IList<BusinessUnitPartRecord> Childs
        {
            get
            {
                return this.Record.Childs;
            }
        }
    }
}