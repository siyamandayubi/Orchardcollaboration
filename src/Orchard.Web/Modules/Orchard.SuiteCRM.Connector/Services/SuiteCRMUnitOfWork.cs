using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Services
{
    public class SuiteCRMUnitOfWork : ISuiteCRMUnitOfWork, IDisposable
    {
        protected SuiteCRMDataContext context { get; private set; }
        protected bool OwesContext = false;

        public SuiteCRMUnitOfWork(DbConnection connection)
        {
            this.context = new SuiteCRMDataContext(connection, true);
            this.OwesContext = true;
        }

        public SuiteCRMUnitOfWork(ISuiteCRMUnitOfWork repository)
        {
            SuiteCRMUnitOfWork otherRepository = (SuiteCRMUnitOfWork)repository;
            this.context = otherRepository.context;
        }

        public void Dispose()
        {
            if (this.OwesContext)
            {
                this.context.Dispose();
            }
        }

        public DbContextTransaction BeginTransaction()
        {
            return this.context.Database.BeginTransaction();
        }
        
        public void Save()
        {
            try
            {
                this.context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class SuiteCRMUnitOfWork<T> : SuiteCRMUnitOfWork, ISuiteCRMUnitOfWork<T>
        where T : class, IEntity, new()
    {
        protected DbSet<T> set;

        public SuiteCRMUnitOfWork(DbConnection connection)
            : base(connection)
        {
            this.set = this.context.Set<T>();
        }

        public SuiteCRMUnitOfWork(ISuiteCRMUnitOfWork repository)
            : base(repository)
        {
            this.set = this.context.Set<T>();
        }

        public void Add(T t)
        {
            this.set.Add(t);
            t.date_entered = DateTime.UtcNow;
            t.id = Guid.NewGuid().ToString().ToLower();
            t.deleted = 0;
        }

        public void Edit(T t)
        {
            this.set.Attach(t);
            t.date_modified = DateTime.UtcNow;
        }

        public void Delete(T t)
        {
            this.set.Remove(t);
        }
    }
}