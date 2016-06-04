using Orchard.UI.Resources;

namespace Bootstrap {
    public class ResourceManifest : IResourceManifestProvider {
        public const string BootswatchStyle = "BootswatchStyle";
        public const string CustomStyle = "CustomStyle";

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            // core styles
            manifest.DefineStyle(BootswatchStyle).SetUrl("bootswatch.min.css", "bootswatch.min.css");
            manifest.DefineStyle(CustomStyle).SetUrl("overwrite.css", "overwrite.css");

            // swatch
            manifest.DefineStyle("bootstrap.min.css").SetUrl("bootstrap.min.css", "bootstrap.min.css");
            manifest.DefineStyle("amelia.min.css").SetUrl("amelia.min.css", "amelia.min.css");
            manifest.DefineStyle("cerulean.min.css").SetUrl("cerulean.min.css", "cerulean.min.css");
            manifest.DefineStyle("cosmo.min.css").SetUrl("cosmo.min.css", "cosmo.min.css");
            manifest.DefineStyle("cyborg.min.css").SetUrl("cyborg.min.css", "cyborg.min.css");
            manifest.DefineStyle("flatly.min.css").SetUrl("flatly.min.css", "flatly.min.css");
            manifest.DefineStyle("journal.min.css").SetUrl("journal.min.css", "journal.min.css");
            manifest.DefineStyle("readable.min.css").SetUrl("readable.min.css", "readable.min.css");
            manifest.DefineStyle("simplex.min.css").SetUrl("simplex.min.css", "simplex.min.css");
            manifest.DefineStyle("slate.min.css").SetUrl("slate.min.css", "slate.min.css");
            manifest.DefineStyle("spacelab.min.css").SetUrl("spacelab.min.css", "spacelab.min.css");
            manifest.DefineStyle("united.min.css").SetUrl("united.min.css", "united.min.css");
        }
    }
}

