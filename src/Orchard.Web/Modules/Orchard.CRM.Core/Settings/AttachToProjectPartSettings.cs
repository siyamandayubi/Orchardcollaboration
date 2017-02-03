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