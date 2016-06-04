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
using Orchard.CRM.Core.Services;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;

namespace Orchard.CRM.Core.Providers.Filters
{
    public class ContentItemPermissionForm : IFormProvider
    {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        protected readonly IContentManager contentManager;
        protected readonly IContentOwnershipHelper contentOwnershipHelper;

        public ContentItemPermissionForm(
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IContentOwnershipHelper contentOwnershipHelper)
        {
            this.contentOwnershipHelper = contentOwnershipHelper;
            this.contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context, string prefix)
        {
            var model = this.contentOwnershipHelper.CreateModel();

            Func<IShapeFactory, object> form = shape =>
            {
                return Shape.ContentItemPermissionView(model);
            };

            context.Form("ContentItemPermission", form);
        }

        public void Describe(DescribeContext context)
        {
            Describe(context, string.Empty);
        }
    }
}