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
