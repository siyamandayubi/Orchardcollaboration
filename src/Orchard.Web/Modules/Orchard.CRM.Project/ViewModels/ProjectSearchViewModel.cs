using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.ViewModels
{
    public class ProjectSearchViewModel
    {
        public string Query { get; set; }
        public int TotalItemCount { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public dynamic ContentItems { get; set; }
        public dynamic Pager { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ContentTypes { get; set; }
    }
}