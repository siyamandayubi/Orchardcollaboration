
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Drivers
{
    public class ProjectDriver : ContentPartDriver<ProjectPart>
    {
        public ProjectDriver()
        {

        }

        protected override DriverResult Display(ProjectPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType == "SyncTitleSummary")
            {
                return ContentShape("Parts_SyncProjectTitle_Summary",
                    () => shapeHelper.Parts_SyncProjectTitle_Summary(
                        Model: part));
            }

            return null;
        }
    }
}