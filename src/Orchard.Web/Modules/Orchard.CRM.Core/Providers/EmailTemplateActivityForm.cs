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
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Providers
{
    public class EmailTemplateActivityForm : IFormProvider
    {
        public const string Name = "EmailTemplateActivityForm";
        public const string EmailTemplateIdFieldName = "EmailTemplateId";
        public const string SentToRequestingUserFieldName = "SentToRequestingUser";
        public const string SentToSharedUsersFieldName = "SentToSharedUsers";
        public const string SentToOwnerFieldName = "SentToOwner";

        private IRepository<EmailTemplateRecord> emailTemplateRepository;

        public EmailTemplateActivityForm(
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IRepository<EmailTemplateRecord> emailTemplateRepository)
        {
            this.emailTemplateRepository = emailTemplateRepository;
            this.contentManager = contentManager;
            this.Shape = shapeFactory;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        protected IContentManager contentManager;
        protected dynamic Shape { get; set; }

        public void Describe(DescribeContext context)
        {
            Func<IShapeFactory, dynamic> formFactory =
                shapeFactory =>
                {
                    var emailTempalates = this.emailTemplateRepository.Table.ToList();

                    var model = emailTempalates.Select(c => new EmailTemplateViewModel
                    {
                        EmailTemplateId = c.Id,
                        Name = c.Name,
                        TypeId = c.TypeId,
                        Subject = c.Subject,
                        Text = c.Body,
                        TypeName = Enum.GetName(typeof(EmailTemplateType), c.TypeId)
                    }).ToList();

                    var t = this.Shape.Form(
                                        Id: "EmailTemplateSelector",
                                        _Parts: this.Shape.EmailTemplateActivityForm(
                                            EmailTemplates: model
                                       ));

                    return t;
                };

            context.Form(EmailTemplateActivityForm.Name, formFactory);
        }
    }
}