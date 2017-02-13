using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;

namespace Orchard.CRM.Core.Providers.Filters
{
    public abstract class SimpleTextBoxFilterForm : IFormProvider
    {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        protected string textboxId { get; set; }
        protected string textboxName { get; set; }
        protected string shapeId { get; set; }
        protected string textboxTitle { get; set; }
        protected string textboxDescription { get; set; }
        protected string formName { get; set; }

        public SimpleTextBoxFilterForm(
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
            string id = prefix + this.textboxId;
            string name = prefix + this.textboxName;
            Func<IShapeFactory, object> form =
                shape =>
                {
                    var f = Shape.Form(
                        Id: this.shapeId,
                        _Parts: Shape.TextBox(
                            Id: id, Name: name,
                            Title: T(this.textboxTitle),
                            Description: T(this.textboxDescription)
                            )
                        );

                    return f;
                };

            context.Form(this.formName, form);
        }
    }
}