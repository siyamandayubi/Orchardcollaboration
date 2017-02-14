using System;
using Orchard.UI.Resources;

namespace Orchard.CRM.TimeTracking
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            builder.Add().DefineScript("TimeTrackingComponents").SetUrl("TimeTrackingComponents.js").SetDependencies("jQueryUI", "reactjs", "reactjs_dom", "BaseComponents", "react-bootstrap"); ;
            builder.Add().DefineScript("TimeTrackingWidgets").SetUrl("TimeTracking.js").SetDependencies("TimeTrackingComponents", "jQueryUI", "reactjs", "reactjs_dom", "BaseComponents", "react-bootstrap");
        }
    }
}