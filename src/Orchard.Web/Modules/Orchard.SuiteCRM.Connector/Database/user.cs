namespace Orchard.SuiteCRM.Connector.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class user
    {
        [StringLength(36)]
        [Column(TypeName="CHAR")]
        public string id { get; set; }

        [StringLength(60)]
        [Column(TypeName = "VARCHAR")]
        public string user_name { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        public string user_hash { get; set; }

        public short? system_generated_password { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime? pwd_last_changed { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string authenticate_id { get; set; }

        public short? sugar_login { get; set; }

        [StringLength(30)]
        [Column(TypeName = "VARCHAR")]
        public string first_name { get; set; }

        [StringLength(30)]
        [Column(TypeName = "VARCHAR")]
        public string last_name { get; set; }

        public short? is_admin { get; set; }

        public short? external_auth_only { get; set; }

        public short? receive_notifications { get; set; }

        [Column(TypeName = "TEXT")]
        public string description { get; set; }

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

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string title { get; set; }

        //[StringLength(255)]
        //[Column(TypeName = "VARCHAR")]
        //public string photo { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string department { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string phone_home { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string phone_mobile { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string phone_work { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string phone_other { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR")]
        public string phone_fax { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string status { get; set; }

        [StringLength(150)]
        [Column(TypeName = "VARCHAR")]
        public string address_street { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(100)]
        public string address_city { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string address_state { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string address_country { get; set; }

        [StringLength(20)]
        [Column(TypeName = "VARCHAR")]
        public string address_postalcode { get; set; }

        [Column(TypeName = "TINYINT")]
        public sbyte? deleted { get; set; }

        public short? portal_only { get; set; }

        public short? show_on_employees { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string employee_status { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string messenger_id { get; set; }

        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string messenger_type { get; set; }

        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string reports_to_id { get; set; }

        public short? is_group { get; set; }
    }
}
