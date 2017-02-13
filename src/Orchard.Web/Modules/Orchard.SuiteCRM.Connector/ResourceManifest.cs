using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            builder.Add().DefineScript("SuiteCRMComponents").SetUrl("SuiteCRMComponents.js").SetDependencies("reactjs", "reactjs_dom", "BaseComponents", "react-bootstrap");
            builder.Add().DefineScript("SuiteCRM").SetUrl("SuiteCRM.js").SetDependencies("jQuery", "reactjs", "reactjs_dom", "BaseComponents", "SuiteCRMComponents");
        }
    }
}