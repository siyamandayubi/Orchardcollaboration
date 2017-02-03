using System.Collections.Generic;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class CopyOrchardProjectToSuiteViewModel
    {
        private List<ProjectIdentifiers> projects = new List<ProjectIdentifiers>();

        public List<ProjectIdentifiers> Projects
        {
            get
            {
                return this.projects;
            }
        }

        public class ProjectIdentifiers
        {
            public string SuiteCRMId { get; set; }
            public int? OrchardCollaborationProjectId { get; set; }
            public bool SyncTasks { get; set; }
            public bool DoNotOverrideNewerValues { get; set; }
            public bool SyncSubTasks { get; set; }
        }
    }    
}