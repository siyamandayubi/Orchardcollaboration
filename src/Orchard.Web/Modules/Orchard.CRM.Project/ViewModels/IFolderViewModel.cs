using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public interface IFolderViewModel
    {
        int? ProjectId { get; set; }
        int? FolderId { get; set; }
        string Title { get; set; }
    }
}