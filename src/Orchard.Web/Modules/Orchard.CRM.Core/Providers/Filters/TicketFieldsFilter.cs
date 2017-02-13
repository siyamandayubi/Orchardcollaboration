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
using System.Web.Mvc;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.CRM.Core.Providers.Filters
{
    public class TicketFieldsFilter : IFilterProvider
    {
        public Localizer T { get; set; }
        public const string CategoryName = "Ticket";
        public const string RequestingUserType = "RequestingUser";
        public const string RelatedContentItem = "RelatedContentItem";
        public const string StatusFilter = "Status";
        public const string PriorityFilter = "Priority";
        public const string StatusTypeFilter = "StatusType";
        public const string TicketType = "TicketType";
        public const string TicketDueDateType = "TicketDueDate";

        private Lazy<IBasicDataService> basicDataService;

        public TicketFieldsFilter(Lazy<IBasicDataService> basicDataService)
        {
            this.basicDataService = basicDataService;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe)
        {
            var descriptor = describe.For(CategoryName, T("Ticket Fields"), T("Ticket Fields"));

            // RequestingUser
            descriptor.Element(
                type: RequestingUserType,
                name: new LocalizedString("Requesting User"),
                description: T("Requesting User Selection"),
                filter: (context) => ApplyRequestingUserFilter(context),
                display: context => Display(context, "RequestingUser_Id"),
                form: RequestingUserForm.FormName);

            // RelatedContentItem
            descriptor.Element(
                type: RelatedContentItem,
                name: new LocalizedString("Related ContentItem"),
                description: T("RelatedContentItem Selection"),
                filter: (context) => ApplyRelatedContentItemIdFilter(context),
                display: context => Display(context, "RelatedContentItem_Id"),
                form: RequestingUserForm.FormName);

            // Priority
            descriptor.Element(
                type: PriorityFilter,
                name: new LocalizedString("Priority"),
                description: T("Priority Selection"),
                filter: (context) => ApplyPriorityTypeFilter(context),
                display: context => Display(context, "Priority_Id"),
                form: TicketPriorityForm.FormName);

            // Status
            descriptor.Element(
                type: StatusFilter,
                name: new LocalizedString("Status"),
                description: T("Status Selection"),
                filter: (context) => ApplyStatusFilter(context),
                display: context => Display(context, "StatusRecord_Id"),
                form: TicketStatusForm.FormName
                    );

            // Status Type
            descriptor.Element(
                type: StatusTypeFilter,
                name: new LocalizedString("Status Type"),
                description: T("Status Type Selection"),
                filter: (context) => ApplyStatusTypeFilter(context),
                display: context => Display(context, "StatusTypeRecord_Id"),
                form: TicketStatusTypeFilterForm.FormName);

            // TicketType
            descriptor.Element(
                type: TicketType,
                name: new LocalizedString("Ticket Type"),
                description: T("Ticke Type Selection"),
                filter: (context) => ApplyTicketTypeFilter(context),
                display: context => Display(context, "TicketType_Id"),
                form: TicketTypeForm.FormName);

            // Due Data
            descriptor.Element(
                type: TicketDueDateType,
                name: new LocalizedString("Ticket Due Date"),
                description: T("Ticket Due Date"),
                filter: (context) => ApplyDueDateFilter(context),
                display: context => Display(context, "DueDate"),
                form: TicketDueDateForm.FormName);
        }

        public void ApplyStatusFilter(FilterContext context)
        {
            Action<IHqlExpressionFactory> predicate = null;
            Action<IAliasFactory> alias = null;

            if (context.State.UnStatus != null && (bool)context.State.UnStatus)
            {
                alias = x => x.ContentPartRecord<TicketPartRecord>();
                predicate = x => x.IsNull("StatusRecord");
                context.Query = context.Query.Where(alias, predicate);
                return;
            }

            if (context.State.Status_Id == null || string.IsNullOrEmpty(context.State.Status_Id.Value))
            {
                return;
            }

            this.ApplyFieldQuery(context, "StatusRecord", (int)context.State.Status_Id);
        }

        public void ApplyStatusTypeFilter(FilterContext context)
        {
            Action<IHqlExpressionFactory> predicate = null;
            Action<IAliasFactory> alias = null;

            if (context.State.StatusType_Id == null || string.IsNullOrEmpty(context.State.StatusType_Id.Value))
            {
                return;
            }
  
            bool notEqual = false;

            if (context.State.NotEqual != null)
            {
                bool.TryParse(context.State.NotEqual.ToString(), out notEqual);
            }


            var statusRecords = this.basicDataService.Value.GetStatusRecords();
            int statusTypeId = context.State.StatusType_Id;
            int[] statusIds = statusRecords.Where(c => c.StatusTypeId == statusTypeId).Select(c => c.Id).ToArray();

            if (statusIds.Length == 0)
            {
                return;
            }

            alias = x => x.ContentPartRecord<TicketPartRecord>();

            predicate = x => x.In("StatusRecord.Id", statusIds);

            if (notEqual)
            {
                Action<IHqlExpressionFactory> notPredicate = x => x.Not(predicate);

                // Not also includes empty items
                Action<IHqlExpressionFactory> orPredicate = x => x.Or(d => d.IsNull("StatusRecord"), notPredicate);

                context.Query = context.Query.Where(alias, orPredicate);
            }
            else
            {
                context.Query = context.Query.Where(alias, predicate);
            }
        }

        public void ApplyTicketTypeFilter(FilterContext context)
        {
            if (context.State.TicketType_Id == null || string.IsNullOrEmpty(context.State.TicketType_Id.Value))
            {
                return;
            }

            this.ApplyFieldQuery(context, "TicketType", (int)context.State.TicketType_Id);
        }

        public void ApplyPriorityTypeFilter(FilterContext context)
        {
            if (context.State.Priority_Id == null || string.IsNullOrEmpty(context.State.Priority_Id.Value))
            {
                return;
            }

            this.ApplyFieldQuery(context, "PriorityRecord", (int)context.State.Priority_Id);
        }

        public void ApplyRequestingUserFilter(FilterContext context)
        {
            if (context.State.RequestingUser_Id == null || string.IsNullOrEmpty(context.State.RequestingUser_Id.Value))
            {
                return;
            }

            this.ApplyFieldQuery(context, "RequestingUser", (int)context.State.RequestingUser_Id);
        }

        private void ApplyFieldQuery(FilterContext context, string propertyName, int value)
        {
            bool notEqual = false;

            if (context.State.NotEqual != null )
            {
                bool.TryParse(context.State.NotEqual.ToString(), out notEqual);
            }

            Action<IAliasFactory> alias = x => x.ContentPartRecord<TicketPartRecord>();
            Action<IHqlExpressionFactory> predicate = x => x.Eq(propertyName + ".Id", value);

            if (notEqual)
            {
                Action<IHqlExpressionFactory> notPredicate = x => x.Not(predicate);

                // Not also includes empty items
                Action<IHqlExpressionFactory> orPredicate = x => x.Or(notPredicate, d => d.IsNull(propertyName));

                context.Query = context.Query.Where(alias, orPredicate);
            }
            else
            {
                context.Query = context.Query.Where(alias, predicate);
            }
        }

        public void ApplyRelatedContentItemIdFilter(FilterContext context)
        {
            if (context.State.RelatedContentItemId == null || string.IsNullOrEmpty(context.State.RelatedContentItemId.Value))
            {
                return;
            }

            this.ApplyFieldQuery(context, "RelatedContentItem", (int)context.State.RelatedContentItemId);
        }

        public void ApplyDueDateFilter(FilterContext context)
        {
            if (context.State.MaxDueDate == null && context.State.MinDueDate == null)
            {
                return;
            }

            DateTime? maxDueDate = context.State.MaxDueDate;
            DateTime? minDueDate = context.State.MinDueDate;

            Action<IAliasFactory> alias = x => x.ContentPartRecord<TicketPartRecord>();

            if (maxDueDate.HasValue)
            {
                // shifted to the last second of the day
                maxDueDate = maxDueDate.Value.Subtract(maxDueDate.Value.TimeOfDay).AddDays(1).AddSeconds(-1);
                Action<IHqlExpressionFactory> predicate = x => x.Le("DueDate", maxDueDate.Value);
                context.Query = context.Query.Where(alias, predicate);
            }

            if (minDueDate.HasValue)
            {
                // shifted to the first second of the day
                minDueDate = minDueDate.Value.Subtract(minDueDate.Value.TimeOfDay);

                Action<IHqlExpressionFactory> predicate = x => x.Ge("DueDate", minDueDate.Value);
                context.Query = context.Query.Where(alias, predicate);
            }
        }

        private LocalizedString Display(FilterContext context, string name)
        {
            return T(name);
        }
    }

    public class TicketDueDateForm : SimpleTextBoxFilterForm
    {
        public const string FormName = "TicketDueDate";
        public TicketDueDateForm(IShapeFactory shapeFactory)
            : base(shapeFactory)
        {
            this.formName = FormName;
            this.textboxId = "MaxDueDate";
            this.textboxName = "MaxDueDate";
            this.textboxTitle = "Due Date deadline";
            this.textboxDescription = "Due date";
        }
    }

    public class RequestingUserForm : SimpleTextBoxFilterForm
    {
        public const string FormName = "RequestingUser";
        public RequestingUserForm(IShapeFactory shapeFactory)
            : base(shapeFactory)
        {
            this.formName = FormName;
            this.textboxId = "RequestingUser_Id";
            this.textboxName = "RequestingUser_Id";
            this.textboxTitle = "Id of the requesting user";
            this.textboxDescription = "Id of the requesting user";
        }
    }

    public class TicketStatusForm : BasicDataFilterForm
    {
        private IBasicDataService basicDataService;

        public const string FormName = "TicketPartRecord_StatusFilter";
        public TicketStatusForm(IShapeFactory shapeFactory, IBasicDataService basicDataService)
            : base(shapeFactory)
        {
            this.basicDataService = basicDataService;
            this.formName = FormName;
            this.selectName = "Status_Id";
            this.selectId = "Status_Id";
            this.selectTitle = "Status";
            this.selectDescription = "Status of the record";
            this.selectSize = 5;
        }

        protected override IList<SelectListItem> GetData()
        {
            var items = basicDataService.GetStatusRecords().OrderBy(c => c.OrderId).ToList().Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Name
            }).ToList();

            items.Add(new SelectListItem { Text = "{Request.Form:StatusId}", Value = "{Request.Form:StatusId}" });
            items.Add(new SelectListItem { Text = "{Request.QueryString:StatusId}", Value = "{Request.QueryString:StatusId}" });
            return items;
        }
    }

    public class TicketPriorityForm : BasicDataFilterForm
    {
        private IBasicDataService basicDataService;

        public const string FormName = "TicketPartRecord_PriorityFilter";
        public TicketPriorityForm(IShapeFactory shapeFactory, IBasicDataService basicDataService)
            : base(shapeFactory)
        {
            this.basicDataService = basicDataService;
            this.formName = FormName;
            this.selectName = "Priority_Id";
            this.selectId = "Priority_Id";
            this.selectTitle = "Priority";
            this.selectDescription = "Priority of the ticket";
            this.selectSize = 5;
        }

        protected override IList<SelectListItem> GetData()
        {
            var items = basicDataService.GetPriorities().OrderBy(c => c.OrderId).ToList().Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Name
            }).ToList();

            return items;
        }
    }

    public class TicketTypeForm : BasicDataFilterForm
    {
        private IBasicDataService basicDataService;

        public const string FormName = "TicketPartRecord_TicketTypeFilter";
        public TicketTypeForm(IShapeFactory shapeFactory, IBasicDataService basicDataService)
            : base(shapeFactory)
        {
            this.basicDataService = basicDataService;
            this.formName = FormName;
            this.selectName = "TicketType_Id";
            this.selectId = "TicketType_Id";
            this.selectTitle = "Ticket Type";
            this.selectDescription = "TicketType of the record";
            this.selectSize = 5;
        }

        protected override IList<SelectListItem> GetData()
        {
            var items = this.basicDataService.GetTicketTypes().OrderBy(c => c.OrderId).ToList().Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Name
            }).ToList();

            return items;
        }
    }
}