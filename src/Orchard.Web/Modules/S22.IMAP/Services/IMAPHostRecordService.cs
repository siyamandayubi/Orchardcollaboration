using Orchard.Data;
using S22.IMAP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace S22.IMAP.Services
{
    public class IMAPHostRecordService : IIMAPHostRecordService
    {
        private readonly IRepository<IMAPHostRecord> repository;

        public IMAPHostRecordService(IRepository<IMAPHostRecord> repository)
        {
            this.repository = repository;
        }

        public IMAPHostRecord Get(string host)
        {
            return this.repository.Table.FirstOrDefault(c => c.Host.ToLower() == host.ToLower());
        }

        public IMAPHostRecord Create(string host, uint uid, DateTime from)
        {
            var item = this.repository.Table.FirstOrDefault(c => c.Host.ToLower() == host.ToLower());
            if (item != null)
            {
                throw new ArgumentException("There is an existing record with the Host:" + host);
            }

            item = new IMAPHostRecord();
            item.Host = host;
            item.EmailUid = uid;
            item.FromDate = from;
            this.repository.Create(item);

            return item;
        }

        public void Save(IMAPHostRecord record)
        {
            this.repository.Update(record);
            this.repository.Flush();
        }
    }
}