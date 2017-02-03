using System.Collections.Generic;

namespace Orchard.SuiteCRM.Connector.ViewModels
{
    public class CopyOrchardTasksToSuiteViewModel
    {
        private List<TaskIdentifiers> tickets = new List<TaskIdentifiers>();

        public List<TaskIdentifiers> Tasks
        {
            get
            {
                return this.tickets;
            }
        }

        public class TaskIdentifiers
        {
            public string SuiteCRMId { get; set; }
            public int? OrchardCollaborationTicketId { get; set; }
            public bool DoNotOverrideNewerValues { get; set; }
            public bool SyncSubTasks { get; set; }
            public bool IsProjectTask { get; set; }
        }
    }    
}