namespace TeleSharp.Entities.Inline
{
    public class InlineQueryResultArticle : InlineQueryResult
    {
        public string Type => "article";
        public string Url { get; set; }
        public bool HideUrl { get; set; }
        public string Description { get; set; }
        public int ThumbWidth { get; set; }
        public int ThumbHeight { get; set; }
    }
}