using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Core.Models;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.CRM.Core.Services
{
    public interface IProjectService : IDependency
    {
        IEnumerable<ContentItem> GetProjects(Pager pager);
        int GetProjectsCount();
        ProjectPart GetProject(int id);
        int? GetProjectIdFromQueryString();
        bool IsTicketsRelatedToProjects();
    }
}
