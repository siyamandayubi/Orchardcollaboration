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
using Orchard.ContentManagement.Records;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;
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
    public class ContentItemPermissionFilter : IFilterProvider
    {
        public const string CategoryName = "ContentItemPermission";
        public const string AnySelectedUserTeamBusinessUnit = "ContentItemPermissionPartRecord.ApplyAnyUserOrTeamOrBusinessUnitsFilter";
        public const string UnassignedItems = "ContentItemPermissionPartRecord.ItemsWithoutAnyOwner";
        public const string CurrentUserPermissions = "ContentItemPermissionPartRecord.CurrentUserPermissions";
        public Localizer T { get; set; }

        protected IOrchardServices orchardServices { get; set; }
        protected IBasicDataService basicDataService;
        protected readonly ICRMContentOwnershipService contentOwnershipService;

        public ContentItemPermissionFilter(
            IOrchardServices orchardServices, 
            IBasicDataService basicDataService,
            ICRMContentOwnershipService contentOwnershipService)
        {
            this.contentOwnershipService = contentOwnershipService;
            this.orchardServices = orchardServices;
            this.basicDataService = basicDataService;
            this.T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe)
        {
            var descriptor = describe.For(CategoryName, T("ContentItemPermission Fields"), T("ContentItemPermission Fields"));

            // User
            descriptor.Element(
                type: "ContentItemPermissionPartRecord.User_Id",
                name: new LocalizedString("User"),
                description: T("User Selection"),
                filter: (context) => ApplyUserFilter(context),
                display: context => Display(context, "Selected User"),
                form: UserServiceUserForm.FormName
                    );

            // BusinessUnit
            descriptor.Element(
                type: "ContentItemPermissionPartRecord.BusinessUnit_Id",
                name: new LocalizedString("BusinessUnit"),
                description: T("BusinessUnit Selection"),
                filter: (context) => ApplyBusinessUnitFilter(context),
                display: context => Display(context, "Selected BusinessUnit"),
                form: BusinessUnitSelectionForm.FormName
                    );

            // All items that current user has access to them
            descriptor.Element(
                type: CurrentUserPermissions,
                name: new LocalizedString("Current User Permissions"),
                description: T("All items that current user has access to them"),
                filter: (context) => AllItemsCurrentUserHasAccessFilter(context),
                display: context => Display(context, "Current User Permissions"));

            // All of the selected BusinessUnits, Teams and Users
            descriptor.Element(
                type: AnySelectedUserTeamBusinessUnit,
                name: new LocalizedString("Multiple Users or Teams or BusinessUnitss Filter (System Only)"),
                description: T("All items that belong to any of the given teams, users and businessUnits. (This is only used by the system)"),
                filter: (context) => ApplyAnyUserOrTeamOrBusinessUnitsFilter(context),
                display: context => Display(context, "ApplyAnyUserOrTeamOrBusinessUnitsFilter"),
                form: UserAllItemsFilterForm.FormName
                    );

            // Items without any assignee
            descriptor.Element(
                type: UnassignedItems,
                name: new LocalizedString("Items without any assignee"),
                description: T("All items without any assignee"),
                filter: (context) => ItemsWithoutAnyAsignee(context),
                display: context => Display(context, "Items Without any Asignee")
                    );
        }

        public void AllItemsCurrentUserHasAccessFilter(FilterContext context)
        {
            // Access to nothing in the case user did not login
            if (this.orchardServices.WorkContext.CurrentUser == null)
            {
                Action<IAliasFactory> alias = x => x.ContentItemVersion();
                Action<IHqlExpressionFactory> predicate = x => x.Eq("Id", "-1");
                context.Query = context.Query.Where(alias, predicate);
                return;
            }

            // there is no restriction for advanced users
            if (this.contentOwnershipService.IsCurrentUserAdvanceOperator())
            {
                return;
            }

            Action<IAliasFactory> mainAlias = x => x.ContentPartRecord<ContentItemPermissionPartRecord>().Property("Items", "ContentItemPermissionDetailRecords");
            int userId = this.orchardServices.WorkContext.CurrentUser.Id;

            Action<IHqlExpressionFactory> userPredicate = x => x.Eq("User.Id", userId);

            List<Action<IHqlExpressionFactory>> teamsAndBusinessUnitPredictions = new List<Action<IHqlExpressionFactory>>();

            List<int> teams = this.basicDataService.GetTeamMembers()
                .Where(c => c.UserPartRecord.Id == userId)
                .Select(c => c.TeamPartRecord.Id).ToList();

            foreach (var team in teams)
            {
                teamsAndBusinessUnitPredictions.Add(t => t.Eq("Team.Id", team));
            }

            List<int> businessUnits = this.basicDataService.GetBusinessUnitMembers()
                .Where(c => c.UserPartRecord.Id == userId)
                .Select(c => c.BusinessUnitPartRecord.Id).ToList();

            if (businessUnits.Count > 0)
            {
                teamsAndBusinessUnitPredictions.Add(x => x.In("BusinessUnit.Id", businessUnits.ToArray()));
            }

            Action<IHqlExpressionFactory> mainPredicate = x => x.Disjunction(userPredicate, teamsAndBusinessUnitPredictions.ToArray());

            context.Query = context.Query.Where(mainAlias, mainPredicate);
        }

        public void ApplyAnyUserOrTeamOrBusinessUnitsFilter(FilterContext context)
        {
            Action<IAliasFactory> mainAlias = x => x.ContentPartRecord<ContentItemPermissionPartRecord>().Property("Items", "ContentItemPermissionDetailRecords");
            ApplyAnyUserOrTeamOrBusinessUnitsFilter(context, mainAlias);
        }

        public void ItemsWithoutAnyAsignee(FilterContext context)
        {
            Action<IAliasFactory> mainAlias = x => x.ContentPartRecord<ContentItemPermissionPartRecord>();
            Action<IHqlExpressionFactory> predicate = x => x.Eq("HasOwner", false);
            context.Query = context.Query.Where(mainAlias, predicate);
        }

        private static void ApplyAnyUserOrTeamOrBusinessUnitsFilter(FilterContext context, Action<IAliasFactory> mainAlias)
        {
            IEnumerable<int> users = context.State.Users;
            IEnumerable<int> teams = context.State.Teams;
            IEnumerable<int> businessUnits = context.State.BusinessUnits;
            List<Action<IHqlExpressionFactory>> predictions = new List<Action<IHqlExpressionFactory>>();
            if (users == null && teams == null && businessUnits == null)
            {
                return;
            }

            Action<IHqlExpressionFactory> accessTypePredicate = null;
            if (context.State.AccessType != null)
            {
                int? accessType = context.State.AccessType;
                accessTypePredicate = x => x.Eq("AccessType", accessType);
            }

            if (users != null && users.Count() > 0)
            {
                Action<IHqlExpressionFactory> usersPredicate = x => x.In("User.Id", users.ToArray());
                predictions.Add(usersPredicate);
            }

            if (teams != null && teams.Count() > 0)
            {
                Action<IHqlExpressionFactory> teamsPredicate = x => x.In("Team.Id", teams.ToArray());
                predictions.Add(teamsPredicate);
            }

            if (businessUnits != null && businessUnits.Count() > 0)
            {
                Action<IHqlExpressionFactory> businessUnitsPredicate = x => x.In("BusinessUnit.Id", businessUnits.ToArray());
                predictions.Add(businessUnitsPredicate);
            }

            Action<IHqlExpressionFactory> mainPredicate = null;
            if (predictions.Count == 1)
            {
                mainPredicate = predictions[0];
                context.Query = context.Query.Where(mainAlias, predictions[0]);
            }
            else if (predictions.Count > 1)
            {
                var first = predictions[0];
                predictions.RemoveAt(0);
                mainPredicate = x => x.Disjunction(first, predictions.ToArray());
            }

            if (mainPredicate == null)
            {
                return;
            }

            if (accessTypePredicate != null)
            {
                Action<IHqlExpressionFactory> tempPredicate = x => x.And(mainPredicate, accessTypePredicate);
                context.Query = context.Query.Where(mainAlias, tempPredicate);
            }
            else
            {
                context.Query = context.Query.Where(mainAlias, mainPredicate);
            }
        }

        public void ApplyUserFilter(FilterContext context)
        {
            if (context.State.User_Id == null || string.IsNullOrEmpty(context.State.User_Id.Value))
            {
                return;
            }

            int value = context.State.User_Id;
            this.ApplyFilter(context, "User", "ContentItemPermissionDetailRecords_UserPart", value);
        }

        public void ApplyBusinessUnitFilter(FilterContext context)
        {
            if (context.State.BusinessUnit_Id == null || string.IsNullOrEmpty(context.State.BusinessUnit_Id.Value))
            {
                return;
            }

            int value = context.State.BusinessUnit_Id;
            this.ApplyFilter(context, "BusinessUnit", "ContentItemPermissionDetailRecords_BusinessUnitPart", value);
        }

        private void ApplyFilter(FilterContext context, string property, string aliasName, int? value)
        {
            Action<IAliasFactory> alias = x => x.ContentPartRecord<ContentItemPermissionPartRecord>().Property("Items", "ContentItemPermissionDetailRecords").Property(property, aliasName);
            Action<IHqlExpressionFactory> predicate = x => x.Eq("Id", value);

            context.Query = context.Query.Where(alias, predicate);
        }

        private LocalizedString Display(FilterContext context, string name)
        {
            return T(name);
        }
    }

    public class UserAllItemsFilterForm : SimpleCheckBoxFilterForm
    {
        public const string FormName = "UserAllItemsFilterForm";
        public UserAllItemsFilterForm(IShapeFactory shapeFactory)
            : base(shapeFactory)
        {
            this.formName = FormName;
            this.checkboxId = "AllItemsOfCurrentUser";
            this.checkboxName = "AllItemsOfCurrentUser";
            this.checkboxTitle = "All items of current user";
            this.checkboxDescription = "All items of current user";
        }
    }

    public class BusinessUnitSelectionForm : BasicDataFilterForm
    {
        private IRepository<BusinessUnitPartRecord> businessUnitRepository;

        public const string FormName = "BusinessUnitSelectionForm";
        public BusinessUnitSelectionForm(IShapeFactory shapeFactory, IRepository<BusinessUnitPartRecord> businessUnitRepository)
            : base(shapeFactory)
        {
            this.businessUnitRepository = businessUnitRepository;
            this.formName = FormName;
            this.selectName = "BusinessUnit_Id";
            this.selectId = "BusinessUnit_Id";
            this.selectTitle = "BusinessUnit ID";
            this.selectDescription = "BusinessUnit Selection";
            this.selectSize = 5;
        }

        protected override IList<SelectListItem> GetData()
        {
            var items = businessUnitRepository.Table.OrderBy(c => c.Name).ToList().Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Name
            }).ToList();

            return items;
        }
    }
}