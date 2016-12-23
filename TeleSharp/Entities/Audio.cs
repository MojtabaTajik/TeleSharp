using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSharp.Entities
{
   public class Audio
    {
        public string FileId { get; set; }
        public int Duration { get; set; }
        public string Performer { get; set; }
        public string Title { get; set; }
        public string MimeType { get; set; }
        public int FileSize { get; set; }
    }
}
