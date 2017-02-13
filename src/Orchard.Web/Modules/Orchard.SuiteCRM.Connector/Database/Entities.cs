using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Database
{
    public partial class project : IEntity
    {
    }

    public partial class project_task : IEntity
    {
    }

    public partial class task : IEntity
    {
    }

    public partial class user : IEntity
    {
    }

    public partial class email_addr_bean_rel : IEntity
    {
        [NotMapped]
        public DateTime? date_entered
        {
            get
            {
                return this.date_created;
            }
            set
            {
                this.date_created = value;
            }
        }
    }

    public partial class email_addresses : IEntity
    {
        [NotMapped]
        public DateTime? date_entered
        {
            get
            {
                return this.date_created;
            }
            set
            {
                this.date_created = value;
            }
        }
    }
}