using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
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

    public class TicketsForContentItemPartSettingsEvents : ContentDefinitionEditorEventsBase
    {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition)
        {
            if (definition.PartDefinition.Name == "TicketsForContentItemPart")
            {
                var model = definition.Settings.GetModel<TicketsForContentItemPartSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel upOwnerModel)
        {
            if (builder.Name == "TicketsForContentItemPart")
            {
                var model = new TicketsForContentItemPartSettings();
                if (upOwnerModel.TryUpdateModel(model, "TicketsForContentItemPartSettings", null, null))
                {
                    builder.WithSetting("TicketsForContentItemPartSettings.DoNotRenderTicketsFlipFlop", model.DoNotRenderTicketsFlipFlop.ToString());
                }

                yield return DefinitionTemplate(model);
            }
        }
    }
}