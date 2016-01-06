namespace TeleSharp.Entities
{
    public class Video
    {
        public string FileId { get; set; }
        public int FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Duration { get; set; }
        public PhotoSize Thumb { get; set; }
        public string MimeType { get; set; }
        public string Caption { get; set; }
    }
}