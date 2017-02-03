using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Project.Models;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Providers.SortCriteria;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Orchard.CRM.Project.Providers.SortCriterias
{
    public class MilestoneSortCriteria : ISortCriterionProvider
    {
        public Localizer T { get; set; }

        public MilestoneSortCriteria()
        {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeSortCriterionContext describe)
        {
            var descriptor = describe.For("Milestone", T("Milestone Fields"), T("Milestone Fields"));

            // StartTime
            descriptor.Element(
                type: MilestonePart.StartTimeFieldName,
                name: new LocalizedString("StartDate"),
                description: T("Sort by Milestone StartTime"),
                display: context => Display(context, "Milestone Start Time"),
                sort: context => ApplySortCriteria(context, MilestonePart.StartTimeFieldName),
                form: SortCriterionFormProvider.FormName);

            // EndTime
            descriptor.Element(
                type: MilestonePart.EndTimeFieldName,
                name: new LocalizedString("EndTime"),
                description: T("Sort by Milestone EndTime"),
                display: context => Display(context, "Milestone End Time"),
                sort: context => ApplySortCriteria(context, MilestonePart.EndTimeFieldName),
                form: SortCriterionFormProvider.FormName);
        }

        public void ApplySortCriteria(SortCriterionContext context, string propertyName)
        {
            Action<IAliasFactory> alias = x => x.ContentPartRecord<MilestonePartRecord>();

            context.Query.Join(alias);
            bool ascending = (bool)context.State.Sort;
            DefaultHqlQuery query = context.Query as DefaultHqlQuery;
            context.Query = ascending ?
                context.Query.OrderBy(alias, x => x.Asc(propertyName)) :
                context.Query.OrderBy(alias, x => x.Desc(propertyName));
        }

        private LocalizedString Display(SortCriterionContext context, string name)
        {
            return T(name);
        }
    }
}