using Orchard.CRM.Core.Models;
using Orchard.CRM.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.CRM.Core;

namespace Orchard.CRM.Project.ViewModels
{
    public class FolderWithAncestorsViewModel : FolderViewModel
    {
        public FolderWithAncestorsViewModel()
        {

        }

        public FolderWithAncestorsViewModel(FolderViewModel model)
        {
            this.ProjectId = model.ProjectId;
            this.Title = model.Title;
            this.IsSelected = model.IsSelected;
            this.FolderId = model.FolderId;
            this.Folders.AddRange(model.Folders);
        }
        private List<FolderViewModel> ancestors = new List<FolderViewModel>();

        public ProjectPartRecord Project { get; set; }

        public List<FolderViewModel> Ancestors
        {
            get
            {
                return this.ancestors;
            }
        }
    }
}