namespace TeleSharp.Entities.Inline
{
    public class InlineQueryResultGif : InlineQueryResult
    {
        public string Type => "gif";
        public string GifUrl { get; set; }
        public int GifWidth { get; set; }
        public int GifHeight { get; set; }
        public string Caption { get; set; }
    }
}