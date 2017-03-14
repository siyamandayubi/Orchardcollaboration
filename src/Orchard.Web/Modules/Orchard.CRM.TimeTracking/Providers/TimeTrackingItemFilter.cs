using Orchard.ContentManagement;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.Services;
using Orchard.CRM.TimeTracking.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.CRM.TimeTracking.Providers
{
    public class TimeTrackingItemFilter : IFilterProvider
    {
        public Localizer T { get; set; }
        public const string CategoryName = "TimeTrackingFilter";
        public const string IdFilterType = "Id";

        public TimeTrackingItemFilter()
        {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe)
        {
            var descriptor = describe.For(CategoryName, T("Time Tracking Fields"), T("Time Tracking Fields"));

            // Project
            descriptor.Element(
                type: IdFilterType,
                name: new LocalizedString("User"),
                description: T("User"),
                filter: (context) => ApplyUserIdFilter(context, false),
                display: context => Display(context, "User_Id"),
                form: TimeTrackingUserSelectionForm.FormName
                    );
        }

        public void ApplyUserIdFilter(FilterContext context, bool notEqual)
        {
            if (context.State.User_Id == null || string.IsNullOrEmpty(context.State.User_Id.Value))
            {
                return;
            }

            Action<IAliasFactory> alias = x => x.ContentPartRecord<TimeTrackingItemRecord>();
            Action<IHqlExpressionFactory> predicate = x => x.Eq("User.Id", context.State.User_Id.Value);

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

    public class TimeTrackingUserSelectionForm : BasicDataFilterForm
    {
        private IBasicDataService basicDataService;
        private IContentManager contentManager;

        public const string FormName = "TimeTrackingUserSelectionForm";
        public TimeTrackingUserSelectionForm(IShapeFactory shapeFactory, IBasicDataService basicDataService, IContentManager contentManager)
            : base(shapeFactory)
        {
            this.contentManager = contentManager;
            this.basicDataService = basicDataService;
            this.formName = FormName;
            this.selectName = "User_Id";
            this.selectId = "User_Id";
            this.selectTitle = "Select User";
            this.selectDescription = "Select an operator user from the list";
            this.selectSize = 5;
        }

        protected override IList<SelectListItem> GetData()
        {
            var operators = this.basicDataService.GetOperators();
            var items = operators.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = CRMHelper.GetFullNameOfUser(c)
            }).ToList();

            // Add token
            items.Add(new SelectListItem { Value = "{User.Id}", Text = "{User.Id}" });
            items.Add(new SelectListItem { Value = "{Request.QueryString:User_Id}", Text = "{Request.QueryString:User_Id}" });
            items.Add(new SelectListItem { Value = "{Request.Form:User_Id}", Text = "{Request.Form:User_Id}" });
            items.Add(new SelectListItem { Value = "{Request.Route:User_Id}", Text = "{Request.Route:User_Id}" });

            return items;
        }
    }
}