using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Models
{
    public class ProjectPart : ContentPart<ProjectPartRecord>
    {
        public int MenuId
        {
            get
            {
                return this.Retrieve(c => c.MenuId);
            }
            set
            {
                this.Store(c => c.MenuId, value);
            }
        }

        public bool RelatedItemsHaveBeenInitialized
        {
            get
            {
                return this.Retrieve(c => c.RelatedItemsHaveBeenInitialized);
            }
            set
            {
                this.Store(c => c.RelatedItemsHaveBeenInitialized, value);
            }
        }
        public string Title
        {
            get
            {
                return this.Record.Title;
            }
            set
            {
                this.Record.Title = value;
            }
        }

        public string Description
        {
            get
            {
                return this.Record.Description;
            }
            set
            {
                this.Record.Description = value;
            }
        }
    }
}