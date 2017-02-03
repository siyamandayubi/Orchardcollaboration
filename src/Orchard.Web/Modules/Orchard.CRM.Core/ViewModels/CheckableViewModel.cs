using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.ViewModels
{
    public class CheckableViewModel<T>
    {
        public T Item { get; set; }
        public bool IsChecked { get; set; }
    }
}