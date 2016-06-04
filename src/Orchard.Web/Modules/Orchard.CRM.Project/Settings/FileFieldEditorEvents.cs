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

namespace Orchard.CRM.Project.Settings {
    public class FileFieldEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "FileField") {
                var model = definition.Settings.GetModel<FileFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new FileFieldSettings();
            if (updateModel.TryUpdateModel(model, "FileFieldSettings", null, null)) {
                builder.WithSetting("FileFieldSettings.Required", model.Required.ToString());
                builder.WithSetting("FileFieldSettings.OpenAction", model.OpenAction.ToString());
                builder.WithSetting("FileFieldSettings.Hint", model.Hint);
                builder.WithSetting("FileFieldSettings.MediaFolder", model.MediaFolder);
                builder.WithSetting("FileFieldSettings.ExtenstionsAllowed", model.ExtenstionsAllowed);
                builder.WithSetting("FileFieldSettings.NameTag", model.NameTag.ToString());
                builder.WithSetting("FileFieldSettings.MaxFileSize", model.MaxFileSize.ToString());
            }

            yield return DefinitionTemplate(model);
        }
    }
}