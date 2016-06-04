/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

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
            builder.Add().DefineScript("reactjs").SetUrl("react-0.14.3.min.js", "react-0.14.3.js");
            builder.Add().DefineScript("reactjs_dom").SetUrl("react-dom-0.14.3.min.js", "react-dom-0.14.3.js");
            builder.Add().DefineScript("react-bootstrap").SetUrl("react-bootstrap.min.js", "react-bootstrap.js").SetDependencies("reactjs", "reactjs_dom");
            builder.Add().DefineScript("jquery-filetype").SetUrl("jquery-filetype.js").SetDependencies("jQueryUI");
            builder.Add().DefineScript("Chosen").SetUrl("chosen/chosen.jquery.js").SetDependencies("jQuery");
            builder.Add().DefineScript("BaseComponents").SetUrl("BaseComponents.js").SetDependencies("reactjs", "reactjs_dom");
            builder.Add().DefineScript("jalert").SetUrl("jalert/jAlert-v3.js").SetDependencies("jQuery");
            builder.Add().DefineScript("jalert-functions").SetUrl("jalert/jAlert-functions.js").SetDependencies("jalert");
            builder.Add().DefineScript("CRMWidgets").SetUrl("CRMWidgets.js").SetDependencies("jQueryUI").SetDependencies("jQuery").SetDependencies("jalert-functions");
           
            var manifest = builder.Add();
            manifest.DefineScript("OrchardTinyMce").SetUrl("tinymce/orchard-tinymce.js").SetDependencies("TinyMce");
        }
    }
}
