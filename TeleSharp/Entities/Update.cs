using TeleSharp.Entities.Inline;

namespace TeleSharp.Entities
{
    public class Update
    {
        public int UpdateId { get; set; }
        public Message Message { get; set; }
        public InlineQuery InlineQuery { get; set; }
        public ChosenInlineResult ChosenInlineResult { get; set; }
    }
}