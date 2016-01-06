namespace TeleSharp.Entities.Inline
{
    public class InlineQueryResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string MessageText { get; set; }
        public string ParseMode { get; set; }
        public string ThumbUrl { get; set; }
        public bool DisableWebPagePreview { get; set; }
    }
}