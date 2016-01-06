namespace TeleSharp.Entities
{
    public class Sticker
    {
        public string FileId { get; set; }
        public int FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public PhotoSize Thumb { get; set; }
    }
}