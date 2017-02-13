using System;
using System.Web.Routing;

namespace Orchard.CRM.Core.Models
{
    public class FileDisplayViewModel
    {
        public RouteValueDictionary RouteValues { get; set; }
        public string Name { get; set; }
        public DateTime Uploaded { get; set; }
    }
}
