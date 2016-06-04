using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Providers
{
    public interface IGroupByParameterProvider : IDependency
    {
        void Describe(DescribeGroupByContext context);
    }
}