using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    /// <summary>
    /// The purpose of the list is tracking which sub-types has custom converter, 
    /// so for such types, the generic converters will not be used
    /// </summary>
    public static class JsonConvertersTypes
    {
        private static List<Type> typesHavingJsonConverters = new List<Type>();

        public static List<Type> TypesHavingJsonConverters
        {
            get
            {
                return typesHavingJsonConverters;
            }
        }
    }
}