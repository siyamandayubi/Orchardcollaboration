using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.WebSite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace OC.GITConnector
{
    public class Initialization : IOrchardShellEvents
    {
        private readonly IAppDataFolder appDataFolder;
        private readonly IWebSiteFolder webSiteFolder;
        private readonly IExtensionManager extensionManager;


        public Initialization(
            IAppDataFolder appDataFolder, 
            IWebSiteFolder webSiteFolder,
            IExtensionManager extensionManager)
        {
            this.extensionManager = extensionManager;
            this.webSiteFolder = webSiteFolder;
            this.appDataFolder = appDataFolder;
        }
        public void Activated()
        {
            var extension = this.extensionManager.GetExtension("OC.GITConnector");
            var path = Path.Combine(extension.Location, extension.Id, "lib/win32/x86/git2-a5cf255.dll").Replace(Path.DirectorySeparatorChar, '/');

            var destination = Path.Combine("Dependencies", "git2-a5cf255.dll");
            if (!this.appDataFolder.FileExists(destination)){
                var stream = new MemoryStream();
                this.webSiteFolder.CopyFileTo(path, stream);
                stream.Seek(0, SeekOrigin.Begin);
                File.WriteAllBytes(appDataFolder.MapPath(destination), stream.ToArray());
            }
        }

        public void Terminating()
        {
        }
    }
}