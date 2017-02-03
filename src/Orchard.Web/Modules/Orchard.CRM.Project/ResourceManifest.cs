using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            manifest.DefineScript("CRMProjectComponents").SetUrl("CRMProjectComponents.js").SetDependencies("reactjs", "reactjs_dom", "BaseComponents", "react-bootstrap");
            manifest.DefineScript("ProjectWidgets").SetUrl("ProjectWidgets.js").SetDependencies("jQueryUI", "CRMProjectComponents");
            manifest.DefineScript("Chosen").SetUrl("chosen/chosen.jquery.js").SetDependencies("jQuery");
            manifest.DefineScript("JsTree").SetUrl("JsTree.js").SetDependencies("jQuery");
            manifest.DefineStyle("JsTree").SetUrl("JsTree/style.css");
        }
    }
}