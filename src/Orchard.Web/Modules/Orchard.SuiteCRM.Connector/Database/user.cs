/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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
