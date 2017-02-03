namespace Orchard.CRM.Core.Settings
{
    using Orchard.ContentManagement.MetaData.Builders;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    public class ActivityStreamPartSettings
    {
        public int PageSize { get; set; }
    
        public void Build(ContentTypePartDefinitionBuilder builder)
        {
            builder.WithSetting("ActivityStreamPartSettings.PageSize", PageSize.ToString(CultureInfo.InvariantCulture));
        }
    }
}