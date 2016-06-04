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
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Common;
    using MySql.Data.Entity;
    using System.Data.Entity.Infrastructure;

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public partial class SuiteCRMDataContext : DbContext
    {
        public SuiteCRMDataContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public virtual DbSet<project> projects { get; set; }
        public virtual DbSet<project_task> project_tasks { get; set; }
        public virtual DbSet<task> tasks { get; set; }
        public virtual DbSet<email_addr_bean_rel> email_addr_bean_rels { get; set; }
        public virtual DbSet<email_addresses> email_addresses { get; set; }
        public virtual DbSet<emails_beans> emails_beans { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<note> notes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>(); 
            base.OnModelCreating(modelBuilder);
        }
    }
}
