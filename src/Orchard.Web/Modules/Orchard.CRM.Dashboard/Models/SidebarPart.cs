using Orchard.ContentManagement;

namespace Orchard.CRM.Dashboard.Models
{
    public class SidebarPart : ContentPart
    {
        /// <summary>
        /// Id of the content that the sidebar hosting that
        /// </summary>
        public int TargetContentItemId
        {
            get { return this.Retrieve(x => x.TargetContentItemId); }
            set { this.Store(x => x.TargetContentItemId, value); }
        }

        /// <summary>
        /// Type of the content that the sidebar hosting that
        /// </summary>
        public string ContentType
        {
            get { return this.Retrieve(x => x.ContentType); }
            set { this.Store(x => x.ContentType, value); }
        }
    }
}