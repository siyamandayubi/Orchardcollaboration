namespace Orchard.CRM.Project.Settings
{
    public class FileFieldSettings {
        public bool Required { get; set; }
        public OpenAction OpenAction { get; set; }
        public string Hint { get; set; }
        public string MediaFolder { get; set; }
        public string ExtenstionsAllowed { get; set; }
        public NameTags NameTag { get; set; }
        public int MaxFileSize { get; set; }
    }

    public enum OpenAction {
        _blank,
        _self,
        _parent,
        _top
    }

    public enum NameTags
    {
        TimeStamp,
        Index
    }
}