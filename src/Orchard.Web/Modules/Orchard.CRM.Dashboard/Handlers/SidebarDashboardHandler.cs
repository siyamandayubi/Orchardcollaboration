using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.CRM.Core.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.CRM.Dashboard.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Services;
using Orchard.Data;
using Orchard.Projections.Models;

namespace Orchard.CRM.Dashboard.Handlers
{
    public class SidebarDashboardHandler : ContentHandler
    {
        public SidebarDashboardHandler(IContentManager contentManager, IRepository<LayoutRecord> layoutRepository)
        {
            OnPublished<SidebarDashboardPart>((context, part) =>
            {
                // portlet list
                var portletNames = string.IsNullOrEmpty(part.SidebarPortletList) ? new string[] { } : part.SidebarPortletList.Split(',');

                var portlets = contentManager.HqlQuery().ForType(Consts.SidebarProjectionPortletTemplateType, Consts.SidebarStaticPortletType).Where(c => c.ContentPartRecord<TitlePartRecord>(), c => c.In("Title", portletNames)).List().Select(c => c.As<TitlePart>());

                // current portlets
                var currentPortlets = contentManager.Query().Where<CommonPartRecord>(c => c.Container.Id == part.ContentItem.Id).List().Select(c => c.As<TitlePart>());

                // add new portlets
                int position = -1;
                foreach (var newPortlet in portletNames.Where(c => !currentPortlets.Any(d => d.Title == c)))
                {
                    position++;
                    var newPortletContentItemTemplate = portlets.FirstOrDefault(c => c.Title == newPortlet);
                    if (newPortletContentItemTemplate == null)
                    {
                        continue;
                    }
                    ContentItem newPortletContentItem = null;

                    if (newPortletContentItemTemplate.ContentItem.ContentType == Consts.SidebarProjectionPortletTemplateType)
                    {
                        newPortletContentItem = contentManager.Create(Consts.SidebarProjectionPortletType);
                        CRMHelper.Copy(layoutRepository, newPortletContentItemTemplate.As<ProjectionWithDynamicSortPart>().Record, newPortletContentItem.As<ProjectionWithDynamicSortPart>().Record);
                    }
                    else
                    {
                        newPortletContentItem = contentManager.Create(Consts.SidebarStaticPortletType);
                        newPortletContentItem.As<BodyPart>().Text = newPortletContentItemTemplate.As<BodyPart>().Text;
                    }

                    // Title
                    newPortletContentItem.As<TitlePart>().Title = newPortletContentItemTemplate.Title;

                    // Container
                    var newPortletCommon = newPortletContentItem.As<CommonPart>();

                    if (newPortletCommon.Container != null && newPortletCommon.Container.Id != part.ContentItem.Id)
                    {
                        continue;
                    }

                    newPortletCommon.Container = part.ContentItem;

                    ContainablePart containablePart = newPortletContentItem.As<ContainablePart>();
                    if (containablePart == null)
                    {
                        continue;
                    }

                    // Position
                    containablePart.Position = position;

                    // publish new portlet
                    contentManager.Publish(newPortletContentItem);
                }

                // delete portlets
                foreach (var portlet in currentPortlets.Where(c => !portletNames.Any(d => d == c.Title)))
                {
                    // Container
                    contentManager.Remove(portlet.ContentItem);
                }


                // set position
                foreach (var portlet in currentPortlets.Where(c => portletNames.Any(d => d == c.Title)))
                {
                    // Container
                    int index = portletNames.ToList().IndexOf(portlet.Title);

                    ContainablePart containablePart = portlet.As<ContainablePart>();
                    if (containablePart == null)
                    {
                        continue;
                    }

                    // Position
                    containablePart.Position = index;

                    // publish new portlet
                    contentManager.Publish(portlet.ContentItem);
                }
            });
        }
    }
}