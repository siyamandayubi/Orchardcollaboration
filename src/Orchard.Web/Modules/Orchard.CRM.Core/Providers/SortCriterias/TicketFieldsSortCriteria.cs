/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

using Orchard.ContentManagement;
using Orchard.CRM.Core.Models;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Providers.SortCriteria;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Orchard.CRM.Core.Providers.SortCriterias
{
    public class TicketFieldsSortCriteria : ISortCriterionProvider
    {
        public Localizer T { get; set; }

        // it will be used for left outer join. We get the method info
        private static MethodInfo DefaultHqlQueryBindCriteriaByAlias = null;

        public TicketFieldsSortCriteria()
        {
            if (DefaultHqlQueryBindCriteriaByAlias == null)
            {
                DefaultHqlQueryBindCriteriaByAlias = (typeof(DefaultHqlQuery)).GetMethod("BindCriteriaByAlias", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeSortCriterionContext describe)
        {
            var descriptor = describe.For("Ticket", T("Ticket Fields"), T("Ticket Fields"));

            // Status
            descriptor.Element(
                type: TicketPart.StatusFieldName,
                name: new LocalizedString("Status"),
                description: T("Sort by status"),
                display: context => Display(context, TicketPart.StatusFieldName),
                sort: context => ApplyStatusFilter(context, "StatusRecord", "OrderId"),
                form: SortCriterionFormProvider.FormName);

            // Priority
            descriptor.Element(
                type: TicketPart.PriorityFieldName,
                name: new LocalizedString("Priority"),
                description: T("Sort by Priority"),
                display: context => Display(context, TicketPart.PriorityFieldName),
                sort: context => ApplyStatusFilter(context, "PriorityRecord", "OrderId"),
                form: SortCriterionFormProvider.FormName);

            // Service
            descriptor.Element(
                type: TicketPart.ServiceFieldName,
                name: new LocalizedString("Service"),
                description: T("Sort by Service"),
                display: context => Display(context, TicketPart.ServiceFieldName),
                sort: context => ApplyStatusFilter(context, "Service", "Name"),
                form: SortCriterionFormProvider.FormName);

            // Identity
            descriptor.Element(
                type: TicketPart.IdentityFieldName,
                name: new LocalizedString("Identity"),
                description: T("Sort by Identity"),
                display: context => Display(context, TicketPart.IdentityFieldName),
                sort: context => ApplyStatusFilter(context, "Identity", "Id"),
                form: SortCriterionFormProvider.FormName);

            // Title
            descriptor.Element(
                type: TicketPart.TitleFieldName,
                name: new LocalizedString("Title"),
                description: T("Sort by Title"),
                display: context => Display(context, TicketPart.TitleFieldName),
                sort: context => ApplyStatusFilter(context, string.Empty, "Title"),
                form: SortCriterionFormProvider.FormName);

            // DueDate
            descriptor.Element(
                type: TicketPart.DueDateFieldName,
                name: new LocalizedString("DueDate"),
                description: T("Sort by DueDate"),
                display: context => Display(context, TicketPart.DueDateFieldName),
                sort: context => ApplyStatusFilter(context, string.Empty, "DueDate"),
                form: SortCriterionFormProvider.FormName);
        }

        public void ApplyStatusFilter(SortCriterionContext context, string partName, string propertyName)
        {
            Action<IAliasFactory> alias = null;

            // if it needs a join
            if (!string.IsNullOrEmpty(partName))
            {
                alias = x =>
                 {
                     x.ContentPartRecord<TicketPartRecord>();
                     DefaultAliasFactory defaultFactory = x as DefaultAliasFactory;

                     // left outer join with the property
                     TicketFieldsSortCriteria.DefaultHqlQueryBindCriteriaByAlias.Invoke(context.Query, new object[] { defaultFactory.Current, partName, partName, "left outer join", null });

                     // it must be recalled again, in order to set the partName as the current Alias
                     x.Property(partName, partName);
                 };
            }
            else
            {
                alias = x => x.ContentPartRecord<TicketPartRecord>();
            }

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