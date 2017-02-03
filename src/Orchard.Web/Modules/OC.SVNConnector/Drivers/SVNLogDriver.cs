using OC.SVNConnector.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace OC.SVNConnector.Drivers
{
    public class SVNLogDriver : ContentPartDriver<SVNLogPart>
    {
        protected override DriverResult Display(SVNLogPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_SVNLog", () => shapeHelper.Parts_SVNLog(Model: part));
        }

        protected override DriverResult Editor(SVNLogPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_SVNLog_Edit",
                             () => shapeHelper.EditorTemplate(
                                 TemplateName: "Parts/SVNLog",
                                 Model: part,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(SVNLogPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}