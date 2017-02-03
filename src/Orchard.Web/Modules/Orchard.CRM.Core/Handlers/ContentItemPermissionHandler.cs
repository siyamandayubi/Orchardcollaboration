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