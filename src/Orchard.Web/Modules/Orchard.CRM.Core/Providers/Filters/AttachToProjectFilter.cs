using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.CRM.Core.Providers.Filters
{
    public class AttachToProjectFilter : IFilterProvider
    {
        public Localizer T { get; set; }
        public const string CategoryName = "AttachToProjectFilter";
        public const string IdFilterType = "Id";

        public AttachToProjectFilter()
        {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe)
        {
            var descriptor = describe.For(CategoryName, T("AttachToProject Fields"), T("AttachToProjectFilter Fields"));

            // Project
            descriptor.Element(
                type: IdFilterType,
                name: new LocalizedString("Related Project"),
                description: T("Related Project"),
                filter: (context) => ApplyProjectIdFilter(context, false),
                display: context => Display(context, "ProjectId_Id"),
                form: ProjectForm.FormName
                    );
        }

        public void ApplyProjectIdFilter(FilterContext context, bool notEqual)
        {
            if (context.State.Project_Id == null || string.IsNullOrEmpty(context.State.Project_Id.Value))
            {
                return;
            }

            Action<IAliasFactory> alias = x => x.ContentPartRecord<AttachToProjectPartRecord>();
            Action<IHqlExpressionFactory> predicate = x => x.Eq("Project.Id", context.State.Project_Id.Value);

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