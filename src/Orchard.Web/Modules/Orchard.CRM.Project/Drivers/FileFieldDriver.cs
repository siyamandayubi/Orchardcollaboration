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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.CRM.Project.Settings;
using Orchard.CRM.Project.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Utility.Extensions;

namespace Orchard.CRM.Project.Drivers
{
    public class FileFieldDriver : ContentFieldDriver<Fields.FileField>
    {
        private const string TokenContentType = "{content-type}";
        private const string TokenFieldName = "{field-name}";
        private const string TokenContentItemId = "{content-item-id}";

        private readonly IStorageProvider _storageProvider;
        public Localizer T { get; set; }

        public FileFieldDriver(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
            T = NullLocalizer.Instance;
        }

        private static string GetPrefix(ContentField field, ContentPart part)
        {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private string GetDifferentiator(ContentField field, ContentPart part)
        {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.FileField field, string displayType, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<FileFieldSettings>();
            return ContentShape("Fields_Project_File", GetDifferentiator(field, part), () => shapeHelper.Fields_Project_File(ContentPart: part, ContentField: field, Settings: settings));
        }

        protected override DriverResult Editor(ContentPart part, Fields.FileField field, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<FileFieldSettings>();

            AssignDefaultMediaFolder(settings);

            var viewModel = new FileFieldViewModel
            {
                Settings = settings,
                Field = field
            };

            return ContentShape("Fields_Project_File_Edit", GetDifferentiator(field, part),
                                () => shapeHelper.EditorTemplate(TemplateName: "Fields/Project.File", Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, Fields.FileField field, IUpdateModel updater, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<FileFieldSettings>();
            var viewModel = new FileFieldViewModel
            {
                Settings = settings,
                Field = field
            };

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null))
            {
                var postedFile = ((Controller)updater).Request.Files["FileField-" + field.Name];

                AssignDefaultMediaFolder(settings);

                var mediaFolder = FormatWithTokens(settings.MediaFolder, part.ContentItem.ContentType, field.Name, part.ContentItem.Id);

                if (postedFile != null && postedFile.ContentLength != 0)
                {

                    var extension = ParseFileExtenstion(postedFile.FileName);
                    var allowedExtensions = ParseAllowedExtention(settings.ExtenstionsAllowed);
                    var fileName = Path.GetFileNameWithoutExtension(postedFile.FileName);

                    if (allowedExtensions.Contains(extension))
                    {
                        if (postedFile.ContentLength <= (settings.MaxFileSize * 1024))
                        {
                            var postedFileLength = postedFile.ContentLength;
                            var postedFileData = new byte[postedFileLength];
                            var postedFileStream = postedFile.InputStream;
                            postedFileStream.Read(postedFileData, 0, postedFileLength);


                            try
                            {
                                // try to create the folder before uploading a file into it
                                _storageProvider.CreateFolder(mediaFolder);
                            }
                            catch
                            {
                                // the folder can't be created because it already exists, continue
                            }

                            if (settings.NameTag == NameTags.Index)
                            {
                                var lastFileIndex =
                                    _storageProvider.ListFiles(mediaFolder)
                                        .Count(f => Path.GetFileNameWithoutExtension(f.GetName()).Contains(fileName));

                                fileName = String.Format("{0} ({1}).{2}", fileName, lastFileIndex + 1, extension);
                            }
                            else if (settings.NameTag == NameTags.TimeStamp)
                                fileName = String.Format("{0}-{1}.{2}", fileName,
                                    DateTime.Now.ToString("yyyyMMddhhmmss"), extension);

                            //
                            var filePath = _storageProvider.Combine(mediaFolder, fileName);
                            var file = _storageProvider.CreateFile(filePath);
                            using (var fileStream = file.OpenWrite())
                            {
                                fileStream.Write(postedFileData, 0, postedFileLength);
                            }

                            field.Path = _storageProvider.GetPublicUrl(file.GetPath());
                        }
                        else
                        {
                            updater.AddModelError("File", T("The file size is bigger than the maximum file size, maximum size is {0}KB.", settings.MaxFileSize));
                        }
                    }
                    else
                    {
                        updater.AddModelError("File", T("The file type is not allowed for {0}.", postedFile.FileName));
                    }

                }
                else
                {
                    if (settings.Required && string.IsNullOrWhiteSpace(field.Path))
                    {
                        updater.AddModelError("File", T("You must provide a file for {0}.", field.Name.CamelFriendly()));
                    }
                }

                if (string.IsNullOrWhiteSpace(field.Text))
                {
                    field.Text = Path.GetFileName(field.Path);
                }
            }

            return Editor(part, field, shapeHelper);
        }

        private IEnumerable<string> ParseAllowedExtention(string allowedExention)
        {
            return allowedExention.Split(',').Select(e => e.Replace(".", "").Trim()).ToArray();
        }

        private string ParseFileExtenstion(string filePath)
        {
            return Path.GetExtension(filePath).Replace(".", "");
        }

        private static string FormatWithTokens(string value, string contentType, string fieldName, int contentItemId)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return String.Empty;
            }

            return value
                .Replace(TokenContentType, contentType)
                .Replace(TokenFieldName, fieldName)
                .Replace(TokenContentItemId, Convert.ToString(contentItemId));
        }

        private static void AssignDefaultMediaFolder(FileFieldSettings settings)
        {
            if (String.IsNullOrWhiteSpace(settings.MediaFolder))
            {
                settings.MediaFolder = TokenContentType + "/" + TokenFieldName;
            }
        }

        private void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}