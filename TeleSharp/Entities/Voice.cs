namespace TeleSharp.Entities
{
    public class Voice
    {
        public string FileId { get; set; }
        public int FileSize { get; set; }
        public int Duration { get; set; }
        public string MimeType { get; set; }
        public string Title { get; set; }
        public string Performer { get; set; }

    }
}