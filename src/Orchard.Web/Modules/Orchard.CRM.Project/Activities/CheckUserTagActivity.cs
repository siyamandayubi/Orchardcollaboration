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

using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.CRM.Project.Services;
using System.Globalization;
using Orchard.CRM.Project.Providers;

namespace Orchard.CRM.Project.Activities
{
    public class CheckUserTagActivity : Task
    {
        public const string Yes = "Yes";
        public const string No = "No";

        public CheckUserTagActivity()
        {
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name
        {
            get { return "CheckUserTag"; }
        }

        public override string Form
        {
            get
            {
                return CheckUserTagActivityForm.Name;
            }
        }

        public override LocalizedString Category
        {
            get { return T("Orchard Collaboration"); }
        }

        public override LocalizedString Description
        {
            get {
                return T("Specifies whether the created user of the content has the specified tag or not");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] {
                T(Yes),
                T(No)
            };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {           
            var commonPart = workflowContext.Content.As<CommonPart>();

            if (commonPart == null || commonPart.Owner == null)
            {
                return new[] { T(No) };
            }

            string specifiedTag = activityContext.GetState<string>(CheckUserTagActivityForm.TagFieldName);

            if (string.IsNullOrEmpty(specifiedTag))
            {
                return new[] { T(Yes) };
            }

            var userTags = ProjectHelper.GetUserField(commonPart.Owner, FieldNames.UserTags);

            if (userTags.ToLower(CultureInfo.InvariantCulture).Contains(specifiedTag.ToLower(CultureInfo.InvariantCulture)))
            {
                return new[] { T(Yes) };
            }

            return new[] { T(No) };
        }
    }
}