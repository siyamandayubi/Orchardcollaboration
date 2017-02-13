using Orchard.ContentManagement;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.ViewModels;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Services
{
    public interface IFolderService : IDependency
    {
        int? GetFolderIdFromQueryString();
        int GetAttachedItemsToFolderCount(int folderId, int projectId);
        IEnumerable<ContentItem> GetFolders(int projectId);
        FolderPart GetFolder(int id);
        FolderViewModel Convert(FolderPart part);
        FolderViewModel FindInTree(FolderViewModel root, int folderId);
        FolderViewModel ConvertToTree(IEnumerable<FolderPart> folders, int? selectedFolderId);
        IList<FolderPart> GetAncestors(IEnumerable<FolderPart> folders, int folderId);
        IList<ContentItem> GetAttachedItemsToFolder(int folderId, int projectId, Pager pager);
        void DeleteFolder(int folderId, bool deleteSubItems);
        int GetAttachedItemsInRootFolderCount(int projectId);
        IList<ContentItem> GetAttachedItemsInRootFolder(int projectId, Pager pager);
    }
}