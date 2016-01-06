namespace TeleSharp.Entities
{
    public class FileDownloadResult : File
    {
        public string FileExtension { get; set; }
        public byte[] Buffer { get; set; }
    }
}