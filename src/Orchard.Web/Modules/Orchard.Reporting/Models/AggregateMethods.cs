using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Models
{
    public enum AggregateMethods
    {
        None = 0,
        Count = 1,
        Sum = 2,
        Average = 3,
        Minimum = 4,
        Maximum = 5
    }
}