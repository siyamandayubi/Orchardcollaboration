using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Database
{
    public class note
    {
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string id { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string assigned_user_id { get; set; }

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

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string name { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string file_mime_type { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string filename { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string parent_type { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string parent_id { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string contact_id { get; set; }

        public short? portal_flag { get; set; }
        public short? embed_flag { get; set; }

        [Column(TypeName = "TEXT")]
        public string description { get; set; }

        [Column(TypeName = "TINYINT")]
        public sbyte? deleted { get; set; }
    }
}