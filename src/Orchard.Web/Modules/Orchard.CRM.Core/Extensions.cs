using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core
{
    public static class Extensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
            {
                collection.Add(item);
            }
        }
    }
}