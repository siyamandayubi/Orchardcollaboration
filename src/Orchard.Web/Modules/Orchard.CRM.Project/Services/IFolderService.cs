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