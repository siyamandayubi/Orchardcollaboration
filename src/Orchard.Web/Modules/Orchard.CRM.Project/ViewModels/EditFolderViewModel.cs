using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class EditFolderViewModel : IFolderViewModel
    {
        public FolderViewModel Tree { get; set; }
        public string Title { get; set; }
        public int? FolderId { get; set; }
        public int? ProjectId { get; set; }
    }
}