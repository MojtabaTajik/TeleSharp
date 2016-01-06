namespace TeleSharp.Entities.Inline
{
    public class InlineQueryResultMpeg4Gif : InlineQueryResult
    {
        public string Type => "mpeg4_gif";
        public string Mpeg4Url { get; set; }
        public int Mpeg4Width { get; set; }
        public int Mpeg4Height { get; set; }
        public string Caption { get; set; }
    }
}