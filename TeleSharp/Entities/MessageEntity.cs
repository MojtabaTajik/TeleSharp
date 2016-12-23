using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSharp.Entities
{
   public class MessageEntity
    {
        public string Type { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Url { get; set; }
        public User User { get; set; }
    }
}
