using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSharp.Entities
{
   public class Game
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<PhotoSize> Photo { get; set; }
        public string Text { get; set; }
        public List<MessageEntity> TextEntities { get; set; }
        public Animation Animation { get; set; }
    }
}
