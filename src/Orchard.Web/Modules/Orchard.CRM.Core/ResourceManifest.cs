using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            builder.Add().DefineScript("spin").SetUrl("spin.js");
            builder.Add().DefineScript("reactjs").SetUrl("react-0.14.3.min.js", "react-0.14.3.js");
            builder.Add().DefineScript("reactjs_dom").SetUrl("react-dom-0.14.3.min.js", "react-dom-0.14.3.js");
            builder.Add().DefineScript("react-bootstrap").SetUrl("react-bootstrap.min.js", "react-bootstrap.js").SetDependencies("reactjs", "reactjs_dom");
            builder.Add().DefineScript("jquery-filetype").SetUrl("jquery-filetype.js").SetDependencies("jQueryUI");
            builder.Add().DefineScript("Chosen").SetUrl("chosen/chosen.jquery.js").SetDependencies("jQuery");
            builder.Add().DefineScript("BaseComponents").SetUrl("BaseComponents.js").SetDependencies("reactjs", "reactjs_dom");
            builder.Add().DefineScript("jalert").SetUrl("jalert/jAlert-v3.js").SetDependencies("jQuery");
            builder.Add().DefineScript("jalert-functions").SetUrl("jalert/jAlert-functions.js").SetDependencies("jalert");
            builder.Add().DefineScript("CRMWidgets").SetUrl("CRMWidgets.js").SetDependencies("jQueryUI").SetDependencies("jQuery").SetDependencies("jalert-functions").SetDependencies("spin");
            builder.Add().DefineStyle("ChosenCss").SetUrl("chosen/chosen.css");

            var manifest = builder.Add();
            manifest.DefineScript("OrchardTinyMce").SetUrl("tinymce/orchard-tinymce.js").SetDependencies("TinyMce");
        }
    }
}
