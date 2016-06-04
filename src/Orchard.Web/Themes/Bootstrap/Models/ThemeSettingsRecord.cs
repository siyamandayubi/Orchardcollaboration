namespace Bootstrap.Models {
    public class ThemeSettingsRecord {
        public ThemeSettingsRecord() {
            this.Swatch = Constants.SwatchCssDefault;
            this.UseFixedNav = true;
            this.UseNavSearch = true;
            this.UseFluidLayout = false;
            this.UseInverseNav = false;
            this.UseStickyFooter = true;
        }

        public virtual int Id { get; set; }
        public virtual string Swatch { get; set; }
        public virtual bool UseFixedNav { get; set; }
        public virtual bool UseNavSearch { get; set; }
        public virtual bool UseFluidLayout { get; set; }
        public virtual bool UseInverseNav { get; set; }
        public virtual bool UseStickyFooter { get; set; }
    }
}