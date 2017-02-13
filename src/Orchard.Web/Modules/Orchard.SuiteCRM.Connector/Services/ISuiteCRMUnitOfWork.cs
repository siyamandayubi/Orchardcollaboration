using Orchard.SuiteCRM.Connector.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Services
{
    /// <summary>
    /// This interface is used in the constructors, in the constructors we need parameterless
    /// </summary>
    public interface ISuiteCRMUnitOfWork
    {
        DbContextTransaction BeginTransaction();
        void Save();
    }

    public interface ISuiteCRMUnitOfWork<T> : ISuiteCRMUnitOfWork, IDisposable
        where T : class ,IEntity, new()
    {
        void Add(T t);
        void Edit(T t);
        void Delete(T t);
    }
}