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

namespace Orchard.CRM.Core.Providers.Filters
{
    public abstract class SimpleCheckBoxFilterForm : IFormProvider
    {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        protected string checkboxId { get; set; }
        protected string checkboxName { get; set; }
        protected string shapeId { get; set; }
        protected string checkboxTitle { get; set; }
        protected string checkboxDescription { get; set; }
        protected string formName { get; set; }

        public SimpleCheckBoxFilterForm(
            IShapeFactory shapeFactory)
        {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context)
        {
            Describe(context, string.Empty);
        }

        public void Describe(DescribeContext context, string prefix)
        {
            string id = prefix + this.checkboxId;
            string name = prefix + this.checkboxName;
            Func<IShapeFactory, object> form =
                shape =>
                {
                    var f = Shape.Form(
                        Id: this.shapeId,
                        _Parts: Shape.Checkbox(
                            Id: id, Name: name,
                            Title: T(this.checkboxTitle),
                            Value: "true",
                            Description: T(this.checkboxDescription)
                            )
                        );

                    return f;
                };

            context.Form(this.formName, form);
        }
    }
}