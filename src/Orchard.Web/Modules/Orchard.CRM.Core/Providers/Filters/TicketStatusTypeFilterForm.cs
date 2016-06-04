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

using Orchard.CRM.Core.Models;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Providers.Filters
{
    public abstract class TicketStatusTypeFilterForm : IFormProvider
    {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public const string FormName = "TicketStatusTypeFilterForm";

        public TicketStatusTypeFilterForm(IShapeFactory shapeFactory)
        {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context)
        {
            this.Describe(context, string.Empty);
        }

        public void Describe(DescribeContext context, string prefix)
        {
            Func<IShapeFactory, object> form =
                shape =>
                {
                    var f = Shape.Form(
                        Id: "StatusType_Id",
                        _Parts: Shape.SelectList(
                        Id: "StatusType_Id",
                        Name: "StatusType_Id",
                            Title: T("Status Type"),
                            Description: T("Type of the Status (Open, Closed etc)"),
                            Size: 4,
                            Multiple: false
                            )
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });
                    f._Parts.Add(new SelectListItem
                    {
                        Value = StatusRecord.NewStatus.ToString(CultureInfo.InvariantCulture),
                        Text = T("New").Text
                    });
                    f._Parts.Add(new SelectListItem
                    {
                        Value = StatusRecord.OpenStatus.ToString(CultureInfo.InvariantCulture),
                        Text = T("In progress").Text
                    });
                    f._Parts.Add(new SelectListItem
                    {
                        Value = StatusRecord.DeferredStatus.ToString(CultureInfo.InvariantCulture),
                        Text = T("Deferred").Text
                    });
                    f._Parts.Add(new SelectListItem
                    {
                        Value = StatusRecord.PendingInputStatus.ToString(CultureInfo.InvariantCulture),
                        Text = T("Pending input").Text
                    });
                    f._Parts.Add(new SelectListItem
                    {
                        Value = StatusRecord.ClosedStatus.ToString(CultureInfo.InvariantCulture),
                        Text = T("Completed").Text
                    });

                    return f;
                };

            context.Form(FormName, form);
        }
    }
}