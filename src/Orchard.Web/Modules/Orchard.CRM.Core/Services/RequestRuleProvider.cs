using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Services
{
    public class RequestRuleProvider : IRuleProvider
    {
        private RequestContext requestContext;
        public RequestRuleProvider(RequestContext requestContext)
        {
            this.requestContext = requestContext;
        }

        public void Process(RuleContext ruleContext)
        {
            if (!String.Equals(ruleContext.FunctionName, "ajax", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (this.requestContext.HttpContext == null || this.requestContext.HttpContext.Request == null)
            {
                return;
            }

            ruleContext.Result = this.requestContext.HttpContext.Request.IsAjaxRequest();
        }
    }
}