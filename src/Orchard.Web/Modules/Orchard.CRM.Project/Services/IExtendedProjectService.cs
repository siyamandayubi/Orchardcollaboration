using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Project.Models;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.CRM.Project.Services
{
    public interface IExtendedProjectService : IProjectService
    {
        void CreateMilestoneAndBacklogForProject(ProjectPart project);
        ContentItem GetProjectRelatedItem(string contentType, int projectId);
        void CreateProjectDependencies(ProjectPart project);
        ContentItem GetProjectWiki(int projectId);
        ContentItem GetProjectMenuWidget(int projectId);
        ContentItem CreateProjectMenu(ProjectPart project);
        ContentItem GetProjectActivityStream(int projectId);
        Dictionary<int, List<ProjectPart>> GetUsersProjects(IEnumerable<int> users);
        void AddPortlet(ContentItem projectDashboard, ContentItem portletTemplate, int position);
        IEnumerable<int> GetDefaultPortletIds(IEnumerable<ContentItem> portletTemplates);
        IEnumerable<ContentItem> GetPortletsTemplates();
    }
}
