using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Widgets.Handlers;
using Orchard.Widgets.Services;

namespace Orchard.CRM.Core.Services
{
    public class ContentEditRuleProvider : IRuleProvider
    {
        private readonly IEditedContentItemHandler editContentItemHandler;

        public ContentEditRuleProvider(IEditedContentItemHandler editContentItemHandler)
        {
            this.editContentItemHandler = editContentItemHandler;
        }

        public void Process(RuleContext ruleContext)
        {
            if (!String.Equals(ruleContext.FunctionName, "editcontenttype", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var contentType = Convert.ToString(ruleContext.Arguments[0]);

            ruleContext.Result = this.editContentItemHandler.IsEdited(contentType);
        }
    }
}