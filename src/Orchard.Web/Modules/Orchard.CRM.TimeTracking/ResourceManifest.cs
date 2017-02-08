using System;
using Orchard.UI.Resources;

namespace Orchard.CRM.TimeTracking
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            builder.Add().DefineScript("TimeTrackingWidgets").SetUrl("TimeTracking.js").SetDependencies("jQueryUI");
        }
    }
}