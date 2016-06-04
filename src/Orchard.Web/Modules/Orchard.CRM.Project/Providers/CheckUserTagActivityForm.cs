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

using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Providers
{
    public class CheckUserTagActivityForm : IFormProvider
    {
        public const string Name = "CheckUsetTagActivityForm";
        public const string TagFieldName = "SelectedTag";

        public CheckUserTagActivityForm(IShapeFactory shapeFactory)
        {
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context)
        {
            Func<IShapeFactory, dynamic> formFactory =
                 shapeFactory =>
                 {
                     var shape = (dynamic)shapeFactory;

                     var t = shape.Form(
                         Id: "TagSelector",
                         _Tag: shape.Textbox(
                            Id: TagFieldName,
                            Name: TagFieldName,
                            Title: T("Tag"),
                            Description: T("Please specify the user Tag that you want the activity to check the created user of the Content against it."),
                            Classes: new[]{"text", "large", "tokenized"}));

                     return t;
                 };

            context.Form(CheckUserTagActivityForm.Name, formFactory);
        }
    }
}