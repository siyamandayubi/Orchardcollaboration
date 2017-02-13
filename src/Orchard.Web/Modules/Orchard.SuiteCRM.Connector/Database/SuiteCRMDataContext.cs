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
