using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSharp.Entities
{
    public class Animation
    {
        public string FileId { get; set; }
        public PhotoSize Thumb { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public int FileSize { get; set; }
    }
}
