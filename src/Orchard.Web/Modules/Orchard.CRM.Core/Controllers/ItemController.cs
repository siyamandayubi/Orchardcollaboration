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

using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.CRM.Core.Models;
using Orchard.Indexing;
using Orchard.Themes;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;

namespace Orchard.CRM.Core.Controllers
{
    [Themed]
    [ValidateInput(false)]
    public class ItemController : BaseController
    {
        public ItemController(
            IIndexProvider indexProvider,
            ICRMContentOwnershipService crmContentOwnershipService,
            IContentOwnershipHelper contentOwnershipHelper,
            IOrchardServices services,
            IExtendedContentManager extendedContentManager,
            ITransactionManager transactionManager,
            IWidgetService widgetService,
            IThemeAwareViewEngine themeAwareViewEngine,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IBasicDataService basicDataService,
            IDisplayHelperFactory displayHelperFactory,
            IActivityStreamService streamService,
            IContentItemDescriptorManager contentItemDescriptorManager)
            : base(string.Empty, string.Empty, indexProvider, services, crmContentOwnershipService, transactionManager, extendedContentManager, contentManager, widgetService, themeAwareViewEngine, shapeFactory, displayHelperFactory, basicDataService, contentOwnershipHelper, streamService, contentItemDescriptorManager)
        {
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override bool IsCreateAuthorized()
        {
            return this.crmContentOwnershipService.IsCurrentUserOperator() || crmContentOwnershipService.IsCurrentUserCustomer();
        }

        protected override bool IsDisplayAuthorized()
        {
            return this.crmContentOwnershipService.IsCurrentUserOperator() || crmContentOwnershipService.IsCurrentUserCustomer();
        }
     }
}