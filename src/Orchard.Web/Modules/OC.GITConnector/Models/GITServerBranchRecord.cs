using System;

namespace OC.GITConnector.Models
{
    public class GITServerBranchRecord
    {
        public virtual int Id { get; set; }
        public virtual GITServerRecord ServerRecord { get; set; }
        public virtual string BranchName { get; set; }
        public virtual string Sha { get; set; }
        public virtual DateTime LastUpdate { get; set; }
    }
}