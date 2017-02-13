using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.CRM.Core.Providers
{
       public class RoleRuleProvider : IRuleProvider
    {
        private readonly IAuthenticationService authenticationService;

        public RoleRuleProvider(IAuthenticationService authenticationService) {
            this.authenticationService = authenticationService;
        }

        public void Process(RuleContext ruleContext) { 
            if (!String.Equals(ruleContext.FunctionName, "role", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var user = authenticationService.GetAuthenticatedUser();
            if (user == null) {
                ruleContext.Result = false;
                return;
            }

            var roles = ruleContext.Arguments.Cast<String>();
            var userRoles = user.As<UserRolesPart>().Roles;
            var matches = userRoles.Intersect(roles, StringComparer.OrdinalIgnoreCase).Count();
            if (matches < 1)
            {
                ruleContext.Result = false;
                return;
            }

            ruleContext.Result = true;
            return;
        }
    }
}
