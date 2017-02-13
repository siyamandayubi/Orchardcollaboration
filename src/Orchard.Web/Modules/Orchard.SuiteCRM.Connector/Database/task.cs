namespace Orchard.SuiteCRM.Connector.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class task
    {
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string id { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string name { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_entered { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_modified { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string modified_user_id { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string created_by { get; set; }

        [Column(TypeName = "TEXT")]
        public string description { get; set; }

        [Column(TypeName = "TINYINT")]
        public sbyte? deleted { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string assigned_user_id { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string status { get; set; }

        public short? date_due_flag { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_due { get; set; }

        public short? date_start_flag { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_start { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string parent_type { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string parent_id { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string contact_id { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string priority { get; set; }
    }
}
