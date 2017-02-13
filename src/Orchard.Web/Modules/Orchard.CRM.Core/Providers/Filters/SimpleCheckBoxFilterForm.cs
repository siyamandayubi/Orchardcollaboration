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