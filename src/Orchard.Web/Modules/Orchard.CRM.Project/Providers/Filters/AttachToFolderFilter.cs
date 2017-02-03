using Orchard.ContentManagement;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Project.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.CRM.Project.Providers.Filters
{
    public class AttachToFolderFilter : IFilterProvider
    {
        public Localizer T { get; set; }
        public const string CategoryName = "AttachToFolder";
        public const string FolderFilterType = "Folder";

        public AttachToFolderFilter()
        {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe)
        {
            var descriptor = describe.For(CategoryName, T("AttachToFolder Fields"), T("AttachToFolder Fields"));

            // Folder Id
            descriptor.Element(
                type: FolderFilterType,
                name: new LocalizedString("Related Folder"),
                description: T("Related Folder"),
                filter: (context) => ApplyFolderFilter(context, false),
                display: context => Display(context, "FolderId_Id"),
                form: FolderSelectionForm.FormName
                    );
        }

        public void ApplyFolderFilter(FilterContext context, bool notEqual)
        {
            if (context.State.Folder_Id == null || string.IsNullOrEmpty(context.State.Folder_Id.Value))
            {
                return;
            }

            string aliasName = "AttachToFolderPartRecord_Folder";
            Action<IAliasFactory> alias = x => x.ContentPartRecord<AttachToFolderPartRecord>().Property("Folder", aliasName);
            Action<IHqlExpressionFactory> predicate = x => x.Eq("Id", context.State.Folder_Id.Value);

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