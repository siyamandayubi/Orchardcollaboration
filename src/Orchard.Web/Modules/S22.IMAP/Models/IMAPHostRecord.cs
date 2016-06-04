using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace S22.IMAP.Models
{
    public class IMAPHostRecord
    {
        public virtual int Id { get; set; }
        public virtual string Host { get; set; }
        public virtual long EmailUid { get; set; }
        public virtual DateTime FromDate { get; set; }
    }
}