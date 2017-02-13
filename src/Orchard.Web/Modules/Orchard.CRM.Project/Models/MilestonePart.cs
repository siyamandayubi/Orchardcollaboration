
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Models
{
    public class MilestonePart : ContentPart<MilestonePartRecord>
    {
        public const string StartTimeFieldName = "StartTime";
        public const string EndTimeFieldName = "EndTime";

        /// <summary>
        /// Specifies whether the milestone is backlog or not
        /// </summary>
        public bool IsBacklog
        {
            get
            {
                return this.Record.IsBacklog;
            }
            set
            {
                this.Record.IsBacklog = value;
            }
        }
    }
}