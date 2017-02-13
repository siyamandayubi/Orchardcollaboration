namespace Orchard.SuiteCRM.Connector.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("project")]
    public partial class project
    {
        [Column(TypeName = "CHAR")]
        [MaxLength(36)]
        [Key]
        public string id { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_entered { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_modified { get; set; }

        [MaxLength(36)]
        [Column(TypeName = "CHAR")]
        public string assigned_user_id { get; set; }

        [MaxLength(36)]
        [Column(TypeName = "CHAR")]
        public string modified_user_id { get; set; }

        [MaxLength(36)]
        [Column(TypeName = "CHAR")]
        public string created_by { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string name { get; set; }

        [Column(TypeName = "TEXT")]
        public string description { get; set; }

        [Column(TypeName = "TINYINT")]
        public sbyte? deleted { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime? estimated_start_date { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime? estimated_end_date { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string status { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string priority { get; set; }
    }
}
