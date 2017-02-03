using Orchard.ContentManagement;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class BusinessUnitMemberPart : ContentPart<BusinessUnitMemberPartRecord>
    {
        public BusinessUnitPartRecord BusinessUnit
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