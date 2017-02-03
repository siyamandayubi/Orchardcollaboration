using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.CRM.Core.Settings 
{
    public class FileUploadPartSettingsEvents : ContentDefinitionEditorEventsBase
    {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) 
        {
            if(definition.PartDefinition.Name != "FileUploadPart")
                yield break;

            var settings = definition.Settings.GetModel<FileUploadPartSettings>();
            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) 
        {
            if (builder.Name != "FileUploadPart")
                yield break;

            var settings = new FileUploadPartSettings();
            if (updateModel.TryUpdateModel(settings, "FileUploadPartSettings", null, null)) 
            {
                settings.Build(builder);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
