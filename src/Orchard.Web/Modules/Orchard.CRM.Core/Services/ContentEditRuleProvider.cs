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