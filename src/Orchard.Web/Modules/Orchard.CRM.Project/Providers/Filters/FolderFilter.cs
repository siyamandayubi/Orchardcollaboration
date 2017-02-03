using Orchard.ContentManagement;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Project.Models;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.CRM.Project.Providers.Filters
{
    public class FolderFilter : IFilterProvider
    {
        public Localizer T { get; set; }
        public const string CategoryName = "Folder";
        public const string ProjectFilterType = "Project";

        public FolderFilter()
        {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe)
        {
            var descriptor = describe.For(CategoryName, T("Folder Fields"), T("Folder Fields"));

            // Project
            descriptor.Element(
                type: ProjectFilterType,
                name: new LocalizedString("Project of Folder"),
                description: T("Project of Folder"),
                filter: (context) => ApplyRequestingUserFilter(context, false),
                display: context => Display(context, "Project_Id"),
                form: ProjectForm.FormName
                    );
        }

        public void ApplyRequestingUserFilter(FilterContext context, bool notEqual)
        {
            if (context.State.Project_Id == null || string.IsNullOrEmpty(context.State.Project_Id.Value))
            {
                return;
            }

            string aliasName = "FolderPartRecord_Project";
            Action<IAliasFactory> alias = x => x.ContentPartRecord<FolderPartRecord>().Property("Project", aliasName);
            Action<IHqlExpressionFactory> predicate = x => x.Eq("Id", context.State.Project_Id.Value);

            if (notEqual)
            {
                predicate = x => x.Not(predicate);
            }

            context.Query = context.Query.Where(alias, predicate);
        }

        private LocalizedString Display(FilterContext context, string name)
        {
            return T(name);
        }
    }
}