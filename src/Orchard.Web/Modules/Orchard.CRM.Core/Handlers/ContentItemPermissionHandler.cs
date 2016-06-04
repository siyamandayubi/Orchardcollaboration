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

using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.UI.Zones;
using Orchard.DisplayManagement;
using System.Globalization;

namespace Orchard.CRM.Core.Handlers
{
    public class ContentItemPermissionHandler : ContentHandler
    {
        protected readonly ICRMContentOwnershipService contentOwnershipService;
        protected IOrchardServices orchardServices;
        protected dynamic shapeFactory;
        private readonly ISessionLocator sessionLocator;

        public ContentItemPermissionHandler(
            ISessionLocator sessionLocator,
            IRepository<ContentItemPermissionPartRecord> repository,
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            ICRMContentOwnershipService contentOwnershipService)
        {
            this.shapeFactory = shapeFactory;
            this.orchardServices = orchardServices;
            this.contentOwnershipService = contentOwnershipService;

            Filters.Add(StorageFilter.For(repository));

            this.OnUpdating<ContentItemPermissionPart>((context, part) =>
                {
                    if (part.Record.Items.Count > 0)
                    {
                        part.Record.HasOwner = part
                            .Record
                            .Items
                            .Count(d =>
                                d.AccessType == ContentItemPermissionAccessTypes.Assignee &&
                                (d.User != null || d.Team != null || d.BusinessUnit != null)) > 0;
                    }
                });

            this.OnPublishing<ContentItemPermissionPart>((context, part) =>
            {
                if (part.Record.Items != null)
                {
                    part.Record.HasOwner = part
                        .Record
                        .Items
                        .Count(d =>
                            d.AccessType == ContentItemPermissionAccessTypes.Assignee &&
                            (d.User != null || d.Team != null || d.BusinessUnit != null)) > 0;
                }
            });

            this.OnIndexing<ContentItemPermissionPart>((context, part) =>
                {
                    List<string> allPermissions = new List<string>();
                    List<string> onlyOwnerPermissions = new List<string>();
                    foreach (var record in part.Record.Items)
                    {
                        string itemString = string.Empty;
                        if (record.BusinessUnit != null)
                        {
                            itemString = "B" + record.BusinessUnit.Id.ToString(CultureInfo.InvariantCulture);
                        }
                        else if (record.Team != null)
                        {
                            itemString = "T" + record.Team.Id.ToString(CultureInfo.InvariantCulture);
                        }
                        else if (record.User != null)
                        {
                            itemString = "U" + record.User.Id.ToString(CultureInfo.InvariantCulture);
                        }

                        if (record.AccessType == ContentItemPermissionAccessTypes.Assignee)
                        {
                            onlyOwnerPermissions.Add(itemString);
                        }

                        allPermissions.Add(itemString);
                    }

                    string allPermissionsIndexValue = string.Join(" ", allPermissions);
                    if (allPermissions.Count == 0)
                    {
                        allPermissionsIndexValue = ContentItemPermissionPart.EmptyPermissionSearchFieldName;
                    }

                    string ownerOnlyPermissionsIndexValue = string.Join(" ", onlyOwnerPermissions);
                    if (onlyOwnerPermissions.Count == 0)
                    {
                        ownerOnlyPermissionsIndexValue = ContentItemPermissionPart.EmptyPermissionSearchFieldName;
                    }

                    context.DocumentIndex.Add(ContentItemPermissionPart.PermissionsSearchFieldName, allPermissionsIndexValue).Analyze().Store();
                    context.DocumentIndex.Add(ContentItemPermissionPart.OwnerSearchFieldName, ownerOnlyPermissionsIndexValue).Analyze().Store();
                });

            this.OnGetDisplayShape<ContentItemPermissionPart>((context, part) =>
            {
                if (!this.contentOwnershipService.CurrentUserCanViewContent(part.ContentItem))
                {
                    dynamic layout = (dynamic)context.Layout;
                    layout.Body = this.shapeFactory.UnauthorizedAccessShape();
                    layout.Content = this.shapeFactory.UnauthorizedAccessShape(); ;
                    layout.Head = null;

                    //throw new System.UnauthorizedAccessException();
                }
            });
        }
    }
}