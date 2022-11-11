namespace GoogleDrive.Services.Models
{
    public class GoogleDriveFileModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public object Content { get; set; }
        public string Kind { get; set; }
        public string MimeType { get; set; }
    }
}
