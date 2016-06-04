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