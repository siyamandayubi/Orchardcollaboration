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