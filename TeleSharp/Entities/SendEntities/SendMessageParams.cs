namespace TeleSharp.Entities.SendEntities
{
    public class SendMessageParams
    {
        public string ChatId { get; set; }
        public string Text { get; set; }
        public string ParseMode { get; set; }
        public bool DisableWebPagePreview { get; set; }
        public Message ReplyToMessage { get; set; }
        public ReplyKeyboardMarkup CustomKeyboard { get; set; }
    }
}