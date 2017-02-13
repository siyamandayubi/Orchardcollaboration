using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Services
{
    public interface IMilestoneService : IDependency
    {
        ContentItem GerProjectBacklog(int projectId);
        ContentItem GetMilestone(int id);
        IEnumerable<ContentItem> GetOpenMilestones(int projectId);
        IEnumerable<ContentItem> GetMilestoneItems(int milestoneId, bool onlyNoneCompleted);
    }
}