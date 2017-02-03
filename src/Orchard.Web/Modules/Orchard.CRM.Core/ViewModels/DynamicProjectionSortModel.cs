using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class DynamicProjectionSortModel
    {
        public string Category { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// true means ascending
        /// </summary>
        public bool Direction { get; set; }
    }
}