using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
using Orchard.CRM.Dashboard.Models;

namespace Orchard.CRM.Dashboard.Handlers
{
    public class SidebarHandler : ContentHandler
    {
        public SidebarHandler(IContentManager contentManager)
        {
            OnPublished<CoverWidgetPart>((context, part) =>
            {
                if (part.TargetContentItemId != default(int)) {
                    return;
                }

                if (string.IsNullOrEmpty(part.ContentType))
                {
                    part.TargetContentItemId = default(int);
                    return;
                }

                if (part.TargetContentItemId == default(int))
                {
                    var contentItem = contentManager.Create(part.ContentType, VersionOptions.Draft);

                    part.TargetContentItemId = contentItem.Id;
                    contentManager.Publish(contentItem);
                }
            });
        }
    }
}