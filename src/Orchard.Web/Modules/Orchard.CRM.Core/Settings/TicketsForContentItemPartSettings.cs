using Orchard.ContentManagement.MetaData.Builders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Settings
{
    public class TicketsForContentItemPartSettings
    {
        /// <summary>
        /// Tickets Flipflop is an icon on one of the corners of the screen (depends on setting) which allows user
        /// hide/show related tickets
        /// </summary>
        public bool DoNotRenderTicketsFlipFlop{ get; set; }

        public void Build(ContentTypePartDefinitionBuilder builder)
        {
            builder.WithSetting("TicketsForContentItemPartSettings.DoNotRenderTicketsFlipFlop", DoNotRenderTicketsFlipFlop.ToString(CultureInfo.InvariantCulture));
        }
    }
}