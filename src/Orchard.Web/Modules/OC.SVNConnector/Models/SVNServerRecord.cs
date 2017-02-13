using System;

namespace OC.SVNConnector.Models
{
    public class SVNServerRecord 
    {
        public virtual int Id { get; set; }
        public virtual string Server { get; set; }
        public virtual long LastRevision { get; set; }
        public virtual DateTime FromDate { get; set; }
    }
}