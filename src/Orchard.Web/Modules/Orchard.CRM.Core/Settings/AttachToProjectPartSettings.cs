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

using Orchard.ContentManagement.MetaData.Builders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Settings
{
    public class AttachToProjectPartSettings
    {
        /// <summary>
        /// For some contents, we don't want to represent the project selection, that is the reason
        /// we need such setting
        /// </summary>
        public bool HiddenInEditMode { get; set; }

        public bool HiddenInDisplayModel { get; set; }

        /// <summary>
        /// If true, then the system doesn't write the changes of this instance of AttachToProject into the ActivityStream
        /// </summary>
        public bool IgnoreInActivityStream { get; set; }

        public void Build(ContentTypePartDefinitionBuilder builder)
        {
            builder.WithSetting("AttachToProjectPartSettings.HiddenInEditMode", HiddenInEditMode.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AttachToProjectPartSettings.HiddenInDisplayModel", HiddenInDisplayModel.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AttachToProjectPartSettings.IgnoreInActivityStream", IgnoreInActivityStream.ToString(CultureInfo.InvariantCulture));
        }
    }
}