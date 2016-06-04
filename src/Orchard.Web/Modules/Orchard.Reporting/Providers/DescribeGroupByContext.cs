using Orchard.Localization;
using Orchard.Projections.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Reporting.Providers
{
    public class DescribeGroupByContext
    {
        private readonly Dictionary<string, DescribeGroupByParameterFor> _describes = new Dictionary<string, DescribeGroupByParameterFor>();

        public IEnumerable<TypeDescriptor<GroupByDescriptor>> Describe()
        {
            return _describes.Select(kp => new TypeDescriptor<GroupByDescriptor>
            {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }

        public DescribeGroupByParameterFor For(string category)
        {
            return For(category, null, null);
        }

        public DescribeGroupByParameterFor For(string category, LocalizedString name, LocalizedString description)
        {
            DescribeGroupByParameterFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor))
            {
                describeFor = new DescribeGroupByParameterFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }
}