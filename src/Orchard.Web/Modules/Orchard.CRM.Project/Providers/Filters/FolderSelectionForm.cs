using Orchard.CRM.Core.Providers.Filters;
using Orchard.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Providers.Filters
{
    public class FolderSelectionForm : SimpleTextBoxFilterForm
    {
        public const string FormName = "FolderSelection";
        public FolderSelectionForm(IShapeFactory shapeFactory)
            : base(shapeFactory)
        {
            this.formName = FormName;
            this.textboxId = "Folder_Id";
            this.textboxName = "Folder_Id";
            this.textboxTitle = "Id of the folder";
            this.textboxDescription = "Please enter the Id of the folder";
        }
    }
}