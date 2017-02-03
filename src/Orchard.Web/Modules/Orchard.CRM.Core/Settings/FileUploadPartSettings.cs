namespace Orchard.CRM.Core.Settings
{
    using Orchard.ContentManagement.MetaData.Builders;
    using Orchard.ContentManagement.MetaData.Models;
    using System.Globalization;

    public static class FileUploadPartSettingsExtensions
    {
        public static FileUploadPartSettings GetFileUploadPartSettings(this ContentTypePartDefinition definition)
        {
            if(definition.Settings.Count == 0)
            {
                return definition.PartDefinition.Settings.GetModel<FileUploadPartSettings>();
            }
            return definition.Settings.GetModel<FileUploadPartSettings>();
        }
    }

    public class FileUploadPartSettings
    {
        //How many items can be uploaded per instance
        public int? FileCountLimit { get; set; }

        //Size of each file
        public int? FileSizeLimit { get; set; }

        //Accepted File Types
        public string FileTypes { get; set; }

        //Dump all the files in what directory?
        public string PublicMediaPath { get; set; }

        public bool DisplayFileUploadInDisplayMode { get; set; }

        public bool HideFileUploadInEditModel { get; set; }

        public void Build(ContentTypePartDefinitionBuilder builder)
        {
            builder.WithSetting("FileUploadPartSettings.FileCountLimit", FileCountLimit.HasValue ? FileCountLimit.ToString() : string.Empty);
            builder.WithSetting("FileUploadPartSettings.FileSizeLimit", FileSizeLimit.HasValue ? FileSizeLimit.ToString() : string.Empty);
            builder.WithSetting("FileUploadPartSettings.FileTypes", FileTypes);
            builder.WithSetting("FileUploadPartSettings.PublicMediaPath", PublicMediaPath);

            builder.WithSetting("FileUploadPartSettings.DisplayFileUploadInDisplayMode", DisplayFileUploadInDisplayMode.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("FileUploadPartSettings.HideFileUploadInEditModel", HideFileUploadInEditModel.ToString(CultureInfo.InvariantCulture));
        }
    }
}
