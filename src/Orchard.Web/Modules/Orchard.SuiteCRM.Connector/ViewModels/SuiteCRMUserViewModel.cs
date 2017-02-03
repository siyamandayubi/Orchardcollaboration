using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class SuiteCRMUserViewModel
    {
        public int? OrchardUserId { get; set; }
        public string OrchardUsername { get; set; }
        public string OrchardEmail { get; set; }
        public string SuiteCRMUserId { get; set; }
        public string SuiteCRMUsername { get; set; }
        public string SuiteCRMEmail { get; set; }
        public bool IsSync { get; set; }
    }
}