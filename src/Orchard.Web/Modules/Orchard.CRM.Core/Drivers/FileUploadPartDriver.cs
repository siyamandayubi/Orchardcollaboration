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

namespace Orchard.CRM.Core.Drivers
{
    using Orchard.Alias;
    using Orchard.ContentManagement.Drivers;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.Settings;
    using Orchard.FileSystems.Media;
    using Orchard.MediaLibrary.Services;
    using Orchard.Tokens;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Routing;

    public class FileUploadPartDriver : ContentPartDriver<FileUploadPart>
    {
        private readonly IMediaLibraryService _mediaService;
        private readonly IStorageProvider storageProvider;
        private readonly IAliasService _aliasService;
        private readonly ITokenizer _tokenizer;
        private readonly ICRMContentOwnershipService crmContentOwnershipService;

        public FileUploadPartDriver(
            ICRMContentOwnershipService crmContentOwnershipService,
            IMediaLibraryService mediaService,
            IAliasService aliasService,
            IStorageProvider storageProvider,
            ITokenizer tokenizer)
        {
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.storageProvider = storageProvider;
            _mediaService = mediaService;
            _aliasService = aliasService;
            _tokenizer = tokenizer;
        }

        protected override DriverResult Display(FileUploadPart part, string displayType, dynamic shapeHelper)
        {
            var settings = part.TypePartDefinition.Settings.GetModel<FileUploadPartSettings>();

            switch (displayType)
            {
                case "Detail":
                    var result = new List<DriverResult>();
                    result.Add(FilesList(part, shapeHelper));

                    if (settings.DisplayFileUploadInDisplayMode)
                    {
                        result.Add(this.FileUpload(part, shapeHelper));
                    }

                    return this.Combined(result.ToArray());

                case "Summary":
                    return this.FilesListSummary(part, shapeHelper);

                default:
                    return null;
            }
        }

        protected override DriverResult Editor(FileUploadPart part, dynamic shapeHelper)
        {
            var settings = part.TypePartDefinition.Settings.GetModel<FileUploadPartSettings>();

            if (settings.HideFileUploadInEditModel)
            {
                var model = new FileUpoadViewModel();
                model.Guid = part.Guid != Guid.Empty ? part.Guid.ToString() : Guid.NewGuid().ToString();
             
                //FileUploadHidden
                return ContentShape("Parts_FileUploadHidden_Edit", () =>
                    shapeHelper.EditorTemplate(TemplateName: "Parts/FileUploadHidden", Model: model, Prefix: Prefix));
            }

            var result = new List<DriverResult>();

            result.Add(this.FileUpload(part, shapeHelper));

            if (part.Id > 0 && part.Guid != Guid.Empty)
            {
                result.Add(FilesList(part, shapeHelper));
            }
            else
            {
                result.Add(ContentShape("Parts_FilesList", () => shapeHelper.Parts_FilesList(Files: new List<FileDisplayViewModel>(), CurrentUserHasEditAccess: true)));
            }

            return Combined(result.ToArray());
        }

        protected override DriverResult Editor(FileUploadPart part, Orchard.ContentManagement.IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new FileUpoadViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);
            part.Guid = Guid.Parse(model.Guid);

            return Editor(part, shapeHelper);
        }

        private DriverResult FilesListSummary(FileUploadPart part, dynamic shapeHelper)
        {
            string folder = "Uploads/" + part.Guid;
            int count = 0;
            if (this.storageProvider.FolderExists(folder))
            {
                count = part.Record.FilesCount;
            }

            return ContentShape("Parts_FilesList_Summary", () => shapeHelper.Parts_FilesList_Summary(Count: count));
        }

        private DriverResult FilesList(FileUploadPart part, dynamic shapeHelper)
        {
            if (part.Guid == Guid.Empty) return null;

            var settings = part.TypePartDefinition.GetFileUploadPartSettings();
            string url;
            if (!string.IsNullOrEmpty(settings.PublicMediaPath))
            {
                url = _tokenizer.Replace(settings.PublicMediaPath, new { Content = part.ContentItem });
            }
            else
            {
                url = part.Guid.ToString();
            }

            List<FileDisplayViewModel> files = new List<FileDisplayViewModel>();

            bool currentUserHasEditAccess = this.crmContentOwnershipService.CurrentUserCanEditContent(part);
            string folder = "Uploads/" + part.Guid;
            if (this.storageProvider.FolderExists(folder))
            {
                files = _mediaService.GetMediaFiles(folder).Select(file =>
                {
                    var routeValues = new RouteValueDictionary();
                    routeValues.Add("id", part.Id);
                    routeValues.Add("guid", part.Record.FolderGuid);
                    routeValues.Add("name", file.Name);
                    routeValues.Add("area", "Orchard.CRM.Core");
                    return new FileDisplayViewModel { Name = file.Name, RouteValues = routeValues, Uploaded = file.LastUpdated };
                }).ToList();
            }

            return ContentShape("Parts_FilesList", () => shapeHelper.Parts_FilesList(Files: files, CurrentUserHasEditAccess: currentUserHasEditAccess));
        }

        private DriverResult FileUpload(FileUploadPart part, dynamic shapeHelper)
        {
            var model = new FileUpoadViewModel();
            model.ContentId = part.Id;
            model.ContentType = part.ContentItem.ContentType;
            model.Settings = part.TypePartDefinition.GetFileUploadPartSettings();

            if (part.Id > 0 && part.Guid == Guid.Empty)
            {
                part.Guid = Guid.NewGuid();
            }

            model.Guid = part.Guid != Guid.Empty ? part.Guid.ToString() : Guid.NewGuid().ToString();

            return ContentShape("Parts_FileUpload_Edit", () =>
                shapeHelper.EditorTemplate(TemplateName: "Parts/FileUpload", Model: model, Prefix: Prefix));
        }
    }
}
