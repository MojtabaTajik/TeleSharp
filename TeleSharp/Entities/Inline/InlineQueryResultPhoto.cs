namespace TeleSharp.Entities.Inline
{
    public class InlineQueryResultPhoto : InlineQueryResult
    {
        public string Type => "photo";
        public string PhotoUrl { get; set; }
        public int PhotoWidth { get; set; }
        public int PhotoHeight { get; set; }
        public string Description { get; set; }
        public string Caption { get; set; }
    }
}