using OC.GITConnector.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OC.GITConnector.Drivers
{
    public class GITCommitDriver : ContentPartDriver<GITCommitPart>
    {
        protected override DriverResult Display(GITCommitPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_GITCommit", () => shapeHelper.Parts_GITCommit(Model: part));
        }


        protected override DriverResult Editor(GITCommitPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_GITCommit_Edit",
                             () => shapeHelper.EditorTemplate(
                                 TemplateName: "Parts/GITCommit",
                                 Model: part,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(GITCommitPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}