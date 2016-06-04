using Orchard;
using S22.IMAP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S22.IMAP.Services
{
    public interface IIMAPHostRecordService : IDependency
    {
        IMAPHostRecord Create(string host, uint uid, DateTime from);
        IMAPHostRecord Get(string host);
        void Save(IMAPHostRecord record);
    }
}
