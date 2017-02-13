using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class SuiteCRMViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreationDateTime { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}