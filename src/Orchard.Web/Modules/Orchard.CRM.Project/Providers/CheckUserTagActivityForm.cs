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