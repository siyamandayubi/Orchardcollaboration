namespace Orchard.SuiteCRM.Connector.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class email_addresses
    {
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string id { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string email_address { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string email_address_caps { get; set; }

        public short? invalid_email { get; set; }

        public short? opt_out { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_created { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_modified { get; set; }

        [Column(TypeName = "TINYINT")]
        public sbyte? deleted { get; set; }
    }
}
