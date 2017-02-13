using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Notification
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            builder.Add().DefineScript("CRMNotification").SetUrl("CRMNotification.js").SetDependencies("jQueryUI").SetDependencies("jQuery").SetDependencies("jalert-functions");
        }
    }
}