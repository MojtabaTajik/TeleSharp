namespace TeleSharp.Entities.Inline
{
    public class InlineQueryResultVideo : InlineQueryResult
    {
        public string Type => "video";
        public string VideoUrl { get; set; }
        public string MimeType { get; set; }
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }
        public int VideoDuration { get; set; }
        public string Description { get; set; }
    }
}