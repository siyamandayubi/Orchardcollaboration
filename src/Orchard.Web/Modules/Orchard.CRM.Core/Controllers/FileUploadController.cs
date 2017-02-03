namespace Orchard.CRM.Core.Controllers
{
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.MetaData;
    using Orchard.Core.Title.Models;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.Settings;
    using Orchard.Data;
    using Orchard.FileSystems.Media;
    using Orchard.Localization;
    using Orchard.MediaLibrary.Services;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class FileUploadController : Controller
    {
        private readonly IMediaLibraryService _mediaService;
        private readonly IOrchardServices _services;
        private readonly IStorageProvider storageProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IRepository<FileUploadPartRecord> fileRepository;
        private readonly ICRMContentOwnershipService contentOwnershipService;
        private readonly IFileService fileService;
        private readonly IActivityStreamService activityStreamService;

        public FileUploadController(IMediaLibraryService mediaService,
            IActivityStreamService activityStreamService,
            ICRMContentOwnershipService contentOwnershipService,
            IOrchardServices services,
            IFileService fileService,
            IRepository<FileUploadPartRecord> fileRepository,
            IStorageProvider storageProvider,
            IContentDefinitionManager contentDefinitionManager)
        {
            this.fileService = fileService;
            this.fileRepository = fileRepository;
            this.contentOwnershipService = contentOwnershipService;
            this.storageProvider = storageProvider;
            _mediaService = mediaService;
            this.activityStreamService = activityStreamService;
            _services = services;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult UploadFile(Guid guid, string qqFile, string contentTypeForFile)
        {
            if ((!_services.Authorizer.Authorize(Permissions.CustomerPermission) &&
                !_services.Authorizer.Authorize(Permissions.OperatorPermission)) || guid == Guid.Empty)
            {
                return Json(new { success = false, message = T("You are not permitted to upload files").Text });
            }

            var stream = Request.InputStream;
            if (string.IsNullOrEmpty(Request["qqFile"]))
            {
                var postedFile = Request.Files[0];
                stream = postedFile.InputStream;
                qqFile = postedFile.FileName;
            }

            Dictionary<string, string> errors = new Dictionary<string, string>();
            int filesCount;
            if (!this.fileService.AddFile(qqFile, stream, contentTypeForFile, guid, errors, out filesCount))
            {
                return Json(new { success = false, Errors = errors, message = errors.Count > 0 ? errors.First().Value : string.Empty });
            }

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("guid", guid);
            routeValueDictionary.Add("area", "orchard.crm.core");
            routeValueDictionary.Add("name", qqFile);

            RouteValueDictionary activityStreamRouteValues = new RouteValueDictionary();
            activityStreamRouteValues.Add("action", "Display");
            activityStreamRouteValues.Add("controller", "Item");
            activityStreamRouteValues.Add("area", "orchard.crm.core");

            // Add to activity stream
            var fileRecord = fileRepository.Table.FirstOrDefault(c => c.FolderGuid == guid);
            if (fileRecord != null)
            {
                var item = _services.ContentManager.Get(fileRecord.ContentItemRecord.Id);
                int userId = _services.WorkContext.CurrentUser.Id;
                fileRecord.FilesCount = filesCount + 1;
                fileRepository.Flush();

                activityStreamRouteValues.Add("id", item.Id);

                string changeDescription = string.Format(CultureInfo.CurrentUICulture, "Add a new file '{0}' to '{1}'", qqFile, this.GetContentItemDescriptionForActivityStream(item));
                this.activityStreamService.WriteChangesToStreamActivity(userId, item.Id, item.VersionRecord.Id, new ActivityStreamChangeItem[] { }, changeDescription, activityStreamRouteValues);
            }

            FileDisplayViewModel fileDisplayModel = new FileDisplayViewModel
           {
               Name = qqFile,
               Uploaded = DateTime.UtcNow,
               RouteValues = routeValueDictionary
           };

            dynamic model = new System.Dynamic.ExpandoObject();
            model.Model = fileDisplayModel;
            model.CurrentUserHasEditAccess = true;
            string fileLinkHtml = CRMHelper.RenderPartialViewToString(this, "FileLinkPartial", model);
            return Json(new { success = true, FileLinkHtml = fileLinkHtml }, "text/html");
        }

        public ActionResult DownloadFile(Guid guid, string name)
        {
            var fileRecord = fileRepository.Table.FirstOrDefault(c => c.FolderGuid == guid);

            if (fileRecord != null)
            {
                var item = _services.ContentManager.Get(fileRecord.ContentItemRecord.Id);
                if (item != null)
                {
                    if (!item.Has<FileUploadPart>())
                    {
                        return HttpNotFound();
                    }

                    if (!this.contentOwnershipService.CurrentUserCanEditContent(item))
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
            }
            else
            {
                return HttpNotFound();
            }


            var folders = this.storageProvider.ListFolders("Uploads");
            var folder = folders.SingleOrDefault(x => x.GetName() == guid.ToString());
            if (folder == null)
            {
                return HttpNotFound();
            }


            var file = this.storageProvider.GetFile(folder.GetPath() + "/" + name);
            if (file == null)
            {
                return HttpNotFound();
            }

            byte[] bytes = null;
            using (var stream = file.OpenRead())
            {
                var reader = new BinaryReader(stream);
                bytes = reader.ReadBytes((int)stream.Length);
            }

            return File(bytes, file.GetFileType(), file.GetName());
        }

        public ActionResult DeleteFile(Guid guid, string name, string returnValue)
        {
            var fileRecord = fileRepository.Table.FirstOrDefault(c => c.FolderGuid == guid);
            ContentItem item = null;

            if (fileRecord != null)
            {
                item = _services.ContentManager.Get(fileRecord.ContentItemRecord.Id);
                if (item != null)
                {
                    if (!item.Has<FileUploadPart>())
                    {
                        return HttpNotFound();
                    }

                    if (!this.contentOwnershipService.CurrentUserCanEditContent(item))
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
            }

            var folders = _mediaService.GetMediaFolders("Uploads");
            var folder = folders.SingleOrDefault(x => x.Name == guid.ToString());
            if (folder == null)
            {
                return HttpNotFound();
            }

            if (this.storageProvider.FileExists(this.storageProvider.Combine(folder.MediaPath, name)))
            {
                _mediaService.DeleteFile(folder.MediaPath, name);
            }

            fileRecord.FilesCount = _mediaService.GetMediaFiles(folder.MediaPath).Count();

            if (item != null)
            {
                int userId = _services.WorkContext.CurrentUser.Id;
                string description = string.Format(CultureInfo.CurrentUICulture, "Delete the file '{0}' from '{1}'", name, this.GetContentItemDescriptionForActivityStream(item));
                RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
                routeValueDictionary.Add("action", "Display");
                routeValueDictionary.Add("controller", "Item");
                routeValueDictionary.Add("area", "Orchard.CRM.Core");
                routeValueDictionary.Add("id", fileRecord.ContentItemRecord.Id);
                this.activityStreamService.WriteChangesToStreamActivity(userId, item.Id, item.VersionRecord.Id, new ActivityStreamChangeItem[] { }, description, routeValueDictionary);
            }

            if (Request.IsAjaxRequest())
            {
                return this.Json(new { IsDone = true }, JsonRequestBehavior.AllowGet);
            }
            else if (!string.IsNullOrEmpty(returnValue))
            {
                return Redirect(returnValue);
            }
            else if (fileRecord != null)
            {
                return this.RedirectToAction("Display", "Ticket", new { area = "Orchard.CRM.Core", id = fileRecord.ContentItemRecord.Id });
            }
            else
            {
                return this.RedirectToAction("Search", "Ticket", new { area = "Orchard.CRM.Core" });
            }
        }

        private string GetContentItemDescriptionForActivityStream(ContentItem contentItem)
        {
            var ticketPart = contentItem.As<TicketPart>();
            if (ticketPart != null)
            {
                string identityString = ticketPart.Record.Identity != null ?
                    ticketPart.Record.Identity.Id.ToString(CultureInfo.InvariantCulture) :
                    string.Empty;
                string title = ticketPart.Record.Title;
                return string.Format("{0} - {1}", identityString, title);
            }

            var titlePart = contentItem.As<TitlePart>();

            if (titlePart != null)
            {
                return titlePart.Title;
            }
            else
            {
                return T("ContentItem").Text + "-" + contentItem.Id.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
