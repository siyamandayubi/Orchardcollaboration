using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class AttachToProjectPart : ContentPart<AttachToProjectPartRecord>
    {
        public const string ProjectIdFieldName = "ProjectId";

        public string ProjectName
        {
            get
            {
                if (this.Record.Project == null)
                {
                    return string.Empty;
                }

                return this.Record.Project.Title;
            }
        }
    }
}