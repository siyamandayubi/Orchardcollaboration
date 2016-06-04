/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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
