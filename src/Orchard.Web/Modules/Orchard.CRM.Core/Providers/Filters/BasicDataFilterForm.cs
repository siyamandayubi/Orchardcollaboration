using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.CRM.Core.Providers.Filters
{
    public abstract class BasicDataFilterForm : IFormProvider
    {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        protected string selectId { get; set; }
        protected string selectName { get; set; }
        protected string shapeId { get; set; }
        protected string selectTitle { get; set; }
        protected string selectDescription { get; set; }
        protected int selectSize { get; set; }
        protected bool selectIsMultiple { get; set; }
        protected string formName { get; set; }

        public BasicDataFilterForm(IShapeFactory shapeFactory)
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
            string id = prefix + this.selectId;
            string name = prefix + this.selectName;
            Func<IShapeFactory, object> form =
                shape =>
                {
                    var f = Shape.Form(
                        Id: this.shapeId,
                        _NotEqual: Shape.CheckBox(
                            Id: "NotEqual", 
                            Name: "NotEqual",
                            Title: T("Not Equal"),
                            Description: T("By ticking the checkbox, the filter selects the records without the given value.")
                            ),
                        _Parts: Shape.SelectList(
                            Id: id, Name: name,
                            Title: T(this.selectTitle),
                            Description: T(this.selectDescription),
                            Size: this.selectSize,
                            Multiple: this.selectIsMultiple
                            )
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    foreach (var item in this.GetData())
                    {
                        f._Parts.Add(item);
                    }

                    return f;
                };

            context.Form(this.formName, form);
        }

        protected abstract IList<SelectListItem> GetData();
    }
}