using System.Collections.Generic;

namespace TeleSharp.Entities
{
    public class PhotoSizeArray
    {
        public int TotalCount { get; set; }
        public List<List<PhotoSize>> Photos { get; set; }
    }
}