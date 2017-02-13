using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.CRM.Core.Providers.Filters
{
    public class ProjectFilter : IFilterProvider
    {
        public Localizer T { get; set; }
        public const string CategoryName = "ProjectFilter";
        public const string IdFilterType = "Id";

        public ProjectFilter()
        {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe)
        {
            var descriptor = describe.For(CategoryName, T("Project Fields"), T("Project Fields"));

            // Project
            descriptor.Element(
                type: IdFilterType,
                name: new LocalizedString("Project"),
                description: T("Project"),
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

            Action<IAliasFactory> alias = x => x.ContentPartRecord<ProjectPartRecord>();
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

    public class ProjectForm : BasicDataFilterForm
    {
        private IBasicDataService basicDataService;
        private IContentManager contentManager;

        public const string FormName = "ProjectSelectionForm";
        public ProjectForm(IShapeFactory shapeFactory, IBasicDataService basicDataService, IContentManager contentManager)
            : base(shapeFactory)
        {
            this.contentManager = contentManager;
            this.basicDataService = basicDataService;
            this.formName = FormName;
            this.selectName = "Project_Id";
            this.selectId = "Project_Id";
            this.selectTitle = "Select Project";
            this.selectDescription = "Select a project from the list";
            this.selectSize = 5;
        }

        protected override IList<SelectListItem> GetData()
        {
            var projects = this.contentManager.Query<ProjectPart>().List();
            var items = projects.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Record.Title
            }).ToList();

            // Add token
            items.Add(new SelectListItem { Value = "{Project.Id}", Text = "{Project.Id}" });

            return items;
        }
    }
}