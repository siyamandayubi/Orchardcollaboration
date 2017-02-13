using Orchard.UI.Resources;

namespace Orchard.CRM.Dashboard
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            builder.Add().DefineScript("DashboardComponents").SetUrl("DashboardComponents.js").SetDependencies("reactjs", "reactjs_dom", "BaseComponents", "react-bootstrap");
        }
    }
}