namespace Orchard.SuiteCRM.Connector.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class emails_beans
    {
        [StringLength(36)]
        [Column(TypeName="CHAR")]
        public string id { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string email_id { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string bean_id { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string bean_module { get; set; }

        [Column(TypeName = "TEXT")]
        public string campaign_data { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_modified { get; set; }

        [Column(TypeName = "TINYINT")]
        public sbyte? deleted { get; set; }
    }
}
