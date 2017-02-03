namespace Orchard.SuiteCRM.Connector.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class project_task
    {
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string id { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_entered { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? date_modified { get; set; }

        [Required]
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string project_id { get; set; }

        public int? project_task_id { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string name { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string status { get; set; }

        //[StringLength(255)]
        //[Column(TypeName = "VARCHAR")]
        //public string relationship_type { get; set; }

        [Column(TypeName = "TEXT")]
        public string description { get; set; }

        [Column(TypeName = "TEXT")]
        public string predecessors { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_start { get; set; }

        public int? time_start { get; set; }

        public int? time_finish { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_finish { get; set; }

        public int? duration { get; set; }

        [Column(TypeName = "TEXT")]
        public string duration_unit { get; set; }

        public int? actual_duration { get; set; }

        public int? percent_complete { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_due { get; set; }

        [Column(TypeName = "TIME")]
        public TimeSpan? time_due { get; set; }

        public int? parent_task_id { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string assigned_user_id { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string modified_user_id { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string priority { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string created_by { get; set; }

        public short? milestone_flag { get; set; }

        public int? order_number { get; set; }

        public int? task_number { get; set; }

        public int? estimated_effort { get; set; }

        public int? actual_effort { get; set; }

        [Column(TypeName = "TINYINT")]
        public sbyte? deleted { get; set; }

        public int? utilization { get; set; }
    }
}
