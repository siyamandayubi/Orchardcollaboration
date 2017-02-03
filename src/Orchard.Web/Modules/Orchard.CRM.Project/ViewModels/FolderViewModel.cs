using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class FolderViewModel : IFolderViewModel
    {
        private Collection<FolderViewModel> folders = new Collection<FolderViewModel>();

        public Collection<FolderViewModel> Folders { get { return this.folders; } }
        public bool IsSelected { get; set; }
        public FolderViewModel Parent { get; set; }
        public bool HasSelectedChild { get; set; }
        public int? ProjectId { get; set; }
        public int? FolderId { get; set; }
        public string Title { get; set; }
    }
}