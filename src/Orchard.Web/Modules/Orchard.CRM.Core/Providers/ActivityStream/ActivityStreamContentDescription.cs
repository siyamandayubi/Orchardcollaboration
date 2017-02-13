using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Orchard.CRM.Core.Providers.ActivityStream
{
    public class ActivityStreamContentDescription
    {
        public ActivityStreamContentDescription(string writerName)
        {
            this.WriterName = writerName;
        }

        public string WriterName { get; private set; }
        public int Weight { get; set; }
        public string Description { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
    }
}