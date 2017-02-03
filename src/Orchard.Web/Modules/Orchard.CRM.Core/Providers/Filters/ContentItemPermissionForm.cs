using Orchard.ContentManagement;
using Orchard.CRM.Core.Services;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;

namespace Orchard.CRM.Core.Providers.Filters
{
    public class ContentItemPermissionForm : IFormProvider
    {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        protected readonly IContentManager contentManager;
        protected readonly IContentOwnershipHelper contentOwnershipHelper;

        public ContentItemPermissionForm(
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IContentOwnershipHelper contentOwnershipHelper)
        {
            this.contentOwnershipHelper = contentOwnershipHelper;
            this.contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context, string prefix)
        {
            var model = this.contentOwnershipHelper.CreateModel();

            Func<IShapeFactory, object> form = shape =>
            {
                return Shape.ContentItemPermissionView(model);
            };

            context.Form("ContentItemPermission", form);
        }

        public void Describe(DescribeContext context)
        {
            Describe(context, string.Empty);
        }
    }
}