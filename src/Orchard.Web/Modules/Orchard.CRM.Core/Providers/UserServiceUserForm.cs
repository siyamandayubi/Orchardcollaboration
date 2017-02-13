using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.CRM.Core.Providers
{
     public class UserServiceUserForm : SimpleTextBoxFilterForm
    {
        public const string FormName = "UserServicePartRecord_UserFilter";
        public UserServiceUserForm(IShapeFactory shapeFactory):base(shapeFactory)
        {
            this.formName = FormName;
            this.textboxId = "User_Id";
            this.textboxName = "User_Id";
            this.textboxTitle = "User ID";
            this.textboxDescription = "Id of the User";
        }
    }

}