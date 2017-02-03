using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public class EditedContentItemDetailHandler : ContentHandler, IEditedContentItemHandler 
    {
        private readonly Collection<string> contentTypes = new Collection<string>();

        protected override void BuildEditorShape(BuildEditorContext context)
        {
            contentTypes.Add(context.ContentItem.ContentType);
        }

        public bool IsEdited(string contentType)
        {
            return contentTypes.Contains(contentType);
        }
    }

    public interface IEditedContentItemHandler : IDependency
    {
        bool IsEdited(string contentType);
    }
}