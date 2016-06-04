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
