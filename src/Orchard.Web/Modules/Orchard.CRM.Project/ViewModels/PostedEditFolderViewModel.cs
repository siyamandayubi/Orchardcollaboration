using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class PostedEditFolderViewModel
    {
        public int FolderId { get; set; }

        [Required]
        public string Title { get; set; }
        public int? ParentId { get; set; }
    }
}