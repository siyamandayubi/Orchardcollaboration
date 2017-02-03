using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class PagerParametersWithSortFields : PagerParameters
    {
        public string SortField { get; set; }
        public bool Descending { get; set; }
    }
}