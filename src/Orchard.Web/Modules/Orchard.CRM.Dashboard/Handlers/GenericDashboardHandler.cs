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
using Orchard.ContentManagement.MetaData;
using Orchard.CRM.Dashboard.Services;
using Orchard.Core.Title.Models;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.CRM.Core.Services;
using Orchard.Projections.Models;
using Orchard.Data;
using Orchard.Reporting.Models;

namespace Orchard.CRM.Dashboard.Handlers
{
    public class GenericDashboardHandler : ContentHandler
    {
        public GenericDashboardHandler(
            IContentManager contentManager, 
            IContentDefinitionManager contentDefinitionManager,
            IRepository<LayoutRecord> layoutRepository)
        {
            OnPublished<GenericDashboardPart>((context, part) => {

                if (!part.CreatePortletsOnPublishing)
                {
                    return;
                }

                // set default portlets for DashboardWidget
                if (part.ContentItem.ContentType == Consts.GenericCoverWidgetContentType && string.IsNullOrEmpty(part.PortletList))
                {
                    part.PortletList = string.Join(",", (new string[] { Consts.SmtpPortletContentType, Consts.IMAPTPortletContentType }));
                }

                var portletTypes = Helper.GetGenericDashboardAvailablePortletTypes(contentDefinitionManager);
                var portlets = contentManager.HqlQuery().ForType(portletTypes).List();

                // portlet list
                var portletNames = string.IsNullOrEmpty(part.PortletList) ? new string[] { } : part.PortletList.Split(',');

                // current portlets
                var currentPortlets = contentManager.Query().Where<CommonPartRecord>(c => c.Container.Id == part.ContentItem.Id).List().Select(c => c.As<TitlePart>());

                // add new portlets
                int position = -1;
                foreach (var newPortlet in portletNames.Where(c => !currentPortlets.Any(d => d.Title == c)))
                {
                    position++;
                    ContentTypeDefinition portletType = null;
                    var newPortletContentItemTemplate = portlets.FirstOrDefault(c =>
                            (c.As<TitlePart>() != null && c.As<TitlePart>().Title == newPortlet));
                    if (newPortletContentItemTemplate != null)
                    {
                        portletType = newPortletContentItemTemplate.TypeDefinition;
                    }
                    else if (portletTypes.Any(c=> c == newPortlet))
                    {
                        portletType = contentDefinitionManager.GetTypeDefinition(newPortlet);
                    }

                    if (portletType == null)
                    {
                        continue;
                    }

                    ContentItem newPortletContentItem = null;

                    if (portletType.Name == Consts.GenericProjectionPortletTemplateType)
                    {
                        newPortletContentItem = contentManager.Create(Consts.GenericProjectionPortletType);
                        CRMHelper.Copy(layoutRepository, newPortletContentItemTemplate.As<ProjectionWithDynamicSortPart>().Record, newPortletContentItem.As<ProjectionWithDynamicSortPart>().Record);
                    }
                    else if (portletType.Name == Consts.GenericReportViewerPortletTemplateType)
                    {
                        newPortletContentItem = contentManager.Create(Consts.GenericReportViewerPortletType);
                        CRMHelper.Copy(newPortletContentItemTemplate.As<DataReportViewerPart>().Record, newPortletContentItem.As<DataReportViewerPart>().Record);
                    }
                    else if (portletType.Name == Consts.GenericActivityStreamPortletTemplateType)
                    {
                        newPortletContentItem = contentManager.Create(Consts.GenericActivityStreamPortletType);
                        var sourceActivityStreamPart = newPortletContentItemTemplate.As<ActivityStreamPart>();
                        var destinationActivityStreamPart = newPortletContentItem.As<ActivityStreamPart>();
                        destinationActivityStreamPart.QueryId = sourceActivityStreamPart.QueryId;
                    }
                    else
                    {
                        newPortletContentItem = contentManager.Create(portletType.Settings[Consts.TemplateInstanceType], VersionOptions.Draft);
                    }

                    ContainablePart containablePart = newPortletContentItem.As<ContainablePart>();
                    if (containablePart == null)
                    {
                        continue;
                    }

                    // Position
                    containablePart.Position = position;

                    // Container
                    var newPortletCommon = newPortletContentItem.As<CommonPart>();
                    newPortletCommon.Container = part.ContentItem;

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