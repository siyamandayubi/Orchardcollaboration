using Orchard.CRM.Core.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.CRM.Core.Services;
using Orchard.Security;
using Orchard.ContentManagement;

namespace Orchard.CRM.Core.Drivers
{
    public class CRMCommentDriver : ContentPartDriver<CRMCommentPart>
    {
        private readonly IOrchardServices services;

        public CRMCommentDriver(IOrchardServices services)
        {
            this.services = services;
        }

        protected override DriverResult Display(CRMCommentPart part, string displayType, dynamic shapeHelper)
        {
            if (part.Record.User == null)
            {
                return null;
            }

            var users = this.services.ContentManager.GetMany<IUser>(new[] { part.Record.User.Id }, VersionOptions.Published, QueryHints.Empty);
            var model = Converter.Convert(services, part.Record, users);
            return ContentShape("Parts_CRMComment",
                () => shapeHelper.Parts_CRMComment(
                    Model: model
                    ));
        }
    }
}