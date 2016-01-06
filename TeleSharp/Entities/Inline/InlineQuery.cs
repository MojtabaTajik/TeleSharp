namespace TeleSharp.Entities.Inline
{
    public class InlineQuery
    {
        public string Id { get; set; }
        public User From { get; set; }
        public string Query { get; set; }
        public string Offset { get; set; }
    }
}