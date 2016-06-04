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

using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Providers.Filters;
using Orchard.CRM.Project.ViewModels;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Orchard.CRM.Core;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.ContentManagement.MetaData;

namespace Orchard.CRM.Project.Services
{
    public class FolderService : BaseService, IFolderService
    {
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly IContentDefinitionManager contentDefinitionManager;
        private readonly ISiteService siteService;

        public FolderService(
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            IOrchardServices services,
            ICRMContentOwnershipService crmContentOwnershipService,
            IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort)
            : base(services, projectionManagerWithDynamicSort)
        {
            this.contentDefinitionManager = contentDefinitionManager;
            this.siteService = siteService;
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.Logger = NullLogger.Instance;
            this.T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IList<ContentItem> GetAttachedItemsToFolder(int folderId, int projectId, Pager pager)
        {
            var contentQuery = this.GetAttachedItemsToFolderQuery(projectId, folderId);

            var contentItems = pager != null ?
                contentQuery.Slice((pager.Page > 0 ? pager.Page - 1 : 0) * pager.PageSize, pager.PageSize) :
                contentQuery.List();

            return contentItems.ToArray();
        }

        public int GetAttachedItemsToFolderCount(int folderId, int projectId)
        {
            var contentQuery = this.GetAttachedItemsToFolderQuery(projectId, folderId);

            return contentQuery.Count();
        }

        public int GetAttachedItemsInRootFolderCount(int projectId)
        {
            var contentQuery = this.GetAttachedItemsInRootFolderQuery(projectId);

            return contentQuery.Count();
        }

        public IList<ContentItem> GetAttachedItemsInRootFolder(int projectId, Pager pager)
        {
            var contentQuery = this.GetAttachedItemsInRootFolderQuery(projectId);
            var contentItems = pager != null ?
                contentQuery.Slice((pager.Page > 0 ? pager.Page - 1 : 0) * pager.PageSize, pager.PageSize) :
                contentQuery.List();

            return contentItems.ToArray();
        }

        public void DeleteFolder(int folderId, bool deleteSubItems)
        {
            var folderContentItem = this.services.ContentManager.Get(folderId);
            var folderPart = folderContentItem.As<FolderPart>();

            var folders = this.GetFolders(folderPart.Record.Project.Id).AsPart<FolderPart>().ToList();
            var tree = this.ConvertToTree(folders, folderId);

            if (folderPart != null)
            {
                // delete all items in sub-folders too
                if (deleteSubItems)
                {
                    List<ContentItem> subItems = this.GetAttachedItemsToFolder(folderId, folderPart.Record.Project.Id, null).ToList();
                    Queue<FolderViewModel> queue = new Queue<FolderViewModel>(tree.Folders);

                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        subItems.AddRange(this.GetAttachedItemsToFolder(current.FolderId.Value, current.ProjectId.Value, null));
                        subItems.Add(folders.First(c => c.Id == current.FolderId).ContentItem);

                        foreach (var subFolder in current.Folders)
                        {
                            queue.Enqueue(subFolder);
                        }
                    }

                    this.services.ContentManager.Remove(folderContentItem);
                    foreach (var item in subItems)
                    {
                        this.services.ContentManager.Remove(item);
                    }
                }
                else
                {
                    var subItems = this.GetAttachedItemsToFolder(folderId, folderPart.Record.Project.Id, null).AsPart<AttachToFolderPart>().ToList();

                    if (folderPart.Record.Parent_Id != null)
                    {
                        subItems.ForEach(c => c.Record.Folder = new FolderPartRecord { Id = folderPart.Record.Parent_Id.Value });
                    }
                    else
                    {
                        subItems.ForEach(c => c.Record.Folder = null);
                    }

                    foreach (var subFolder in tree.Folders)
                    {
                        var subFolderPart = folders.First(c => c.Id == subFolder.FolderId.Value);
                        subFolderPart.Record.Parent_Id = folderPart.Record.Parent_Id;
                    }
                }
            }
        }

        public FolderPart GetFolder(int id)
        {
            return this.services.ContentManager.Get<FolderPart>(id);
        }

        public IEnumerable<ContentItem> GetFolders(int projectId)
        {
            return this.GetFolders(projectId, true);
        }

        public FolderViewModel Convert(FolderPart part)
        {
            string title = part.Record.Title;

            return new FolderViewModel
               {
                   Title = title,
                   ProjectId = part.Record.Project != null ? (int?)part.Record.Project.Id : null,
                   FolderId = part.Id > 0 ? (int?)part.Id : null,
               };
        }

        public FolderViewModel ConvertToTree(IEnumerable<FolderPart> folders, int? selectedFolderId)
        {
            FolderViewModel root = new FolderViewModel();
            root.Title = T("Root").Text;
            var temp = folders
                .Where(c => c.Record.Parent_Id == null || !folders.Any(d => d.Record.Id == c.Record.Parent_Id))
                .Select(c => this.Convert(c)).ToList();
            temp.ForEach(c => c.IsSelected = c.FolderId == selectedFolderId);

            root.Folders.AddRange(temp);

            List<FolderViewModel> selectedItems = new List<FolderViewModel>();

            // Create tree
            Queue<FolderViewModel> queue = new Queue<FolderViewModel>(root.Folders);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();

                if (item.IsSelected)
                {
                    selectedItems.Add(item);
                }

                var subFolders = folders.Where(c => c.Record.Parent_Id != null && c.Record.Parent_Id == item.FolderId).ToList();

                foreach (var subFolder in subFolders)
                {
                    FolderViewModel subFolderModel = this.Convert(subFolder);
                    subFolderModel.Parent = item;
                    subFolderModel.IsSelected = subFolderModel.FolderId == selectedFolderId;
                    item.Folders.Add(subFolderModel);
                    queue.Enqueue(subFolderModel);
                }
            }

            // Set HasSelectedChild attribute
            foreach (var selectedItem in selectedItems)
            {
                var tempParent = selectedItem.Parent;
                while (tempParent != null)
                {
                    tempParent.HasSelectedChild = true;
                    tempParent = tempParent.Parent;
                }
            }

            return root;
        }

        public int? GetFolderIdFromQueryString()
        {
            // retrieving paging parameters
            var queryString = this.services.WorkContext.HttpContext.Request.QueryString;
            var folderKey = "folderId";
            int? folder = null;

            if (queryString.AllKeys.Contains(folderKey))
            {
                int temp;
                Int32.TryParse(queryString[folderKey], out temp);
                folder = temp;
            }

            return folder;
        }

        public FolderViewModel FindInTree(FolderViewModel root, int folderId)
        {
            Queue<FolderViewModel> queue = new Queue<FolderViewModel>(new[] { root });

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (item.FolderId == folderId)
                {
                    return item;
                }
                else
                {
                    foreach (var child in item.Folders)
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return null;
        }

        public IList<FolderPart> GetAncestors(IEnumerable<FolderPart> folders, int folderId)
        {
            List<FolderPart> returnValue = new List<FolderPart>();

            var list = folders.ToList();
            var temp = list.FirstOrDefault(c => c.Id == folderId);
            while (temp != null)
            {
                temp = temp.Record.Parent_Id != null ? list.FirstOrDefault(c => c.Record.Id == temp.Record.Parent_Id.Value) : null;

                if (temp != null)
                {
                    returnValue.Add(temp);
                }
            }

            returnValue.Reverse();

            return returnValue;
        }

        private IHqlQuery GetAttachedItemsInRootFolderQuery(int projectId)
        {
            var contentQuery = this.services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);
            dynamic state = new JObject();

            contentQuery = this.ApplyContentPermissionFilter(contentQuery);

            // apply project filter
            state.Project_Id = projectId.ToString(CultureInfo.InvariantCulture);
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, AttachToProjectFilter.CategoryName, AttachToProjectFilter.IdFilterType, state);

            // get user folders
            var userFolders = this.GetFolders(projectId).Select(c => c.As<FolderPart>()).Select(c => c.Record.Id).ToArray();

            if (userFolders.Length > 0)
            {
                // root folder
                Action<IAliasFactory> rootFolderAlias = x => x.ContentPartRecord<AttachToFolderPartRecord>();
                Action<IHqlExpressionFactory> rootFolderPredicate = x => x.IsNull("Folder");

                Action<IHqlExpressionFactory> inFoldersPredicate = x => x.In("Folder.Id", userFolders);
                Action<IHqlExpressionFactory> notPredicate = x => x.Not(inFoldersPredicate);
                Action<IHqlExpressionFactory> orPredicate = x => x.Disjunction(rootFolderPredicate, new[] { notPredicate });
                contentQuery = contentQuery.Where(rootFolderAlias, orPredicate);
            }

            // get types which have AttachToFolderPart
            var types = this.contentDefinitionManager.ListTypeDefinitions().Where(c => c.Parts.Any(d => d.PartDefinition.Name == "AttachToFolderPart")).ToList();
            state = new JObject();
            state.ContentTypes = string.Join<string>(",", types.Select(c => c.Name));
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);

            return contentQuery;
        }

        private IHqlQuery GetAttachedItemsToFolderQuery(int projectId, int folderId)
        {
            var contentQuery = this.services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);
            dynamic state = new JObject();

            contentQuery = this.ApplyContentPermissionFilter(contentQuery);

            // apply project filter
            state.Project_Id = projectId.ToString(CultureInfo.InvariantCulture);
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, AttachToProjectFilter.CategoryName, AttachToProjectFilter.IdFilterType, state);

            // apply folder filter
            state.Folder_Id = folderId.ToString(CultureInfo.InvariantCulture);
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, AttachToFolderFilter.CategoryName, AttachToFolderFilter.FolderFilterType, state);

            return contentQuery;
        }

        private IEnumerable<ContentItem> GetFolders(int projectId, bool restrictToCurrentUserAccess)
        {
            var contentQuery = this.services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);

            if (restrictToCurrentUserAccess)
            {
                contentQuery = this.ApplyContentPermissionFilter(contentQuery);
            }

            // apply project filter
            dynamic state = new JObject();
            state.Project_Id = projectId.ToString(CultureInfo.InvariantCulture);
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, FolderFilter.CategoryName, FolderFilter.ProjectFilterType, state);

            contentQuery.Include(new[] { "FolderPartRecord" });

            state = new JObject();
            state.ContentTypes = ContentTypes.FolderContentType;
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);
            
            var folders = contentQuery.List();

            return folders.ToArray();
        }
    }
}