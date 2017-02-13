namespace Orchard.SuiteCRM.Connector.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class email_addr_bean_rel
    {
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string id { get; set; }

        [Required]
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string email_address_id { get; set; }

        [Required]
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string bean_id { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string bean_module { get; set; }

        public short? primary_address { get; set; }

        public short? reply_to_address { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_created { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_modified { get; set; }

        [Column(TypeName = "TINYINT")]
        public sbyte? deleted { get; set; }
    }
}
