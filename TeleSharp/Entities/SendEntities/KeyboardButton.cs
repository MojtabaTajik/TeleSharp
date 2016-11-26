using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSharp.Entities.SendEntities
{
    public class KeyboardButton
    {
        public string Text { get; set; }
        public bool RequestContact { get; set; }
        public bool RequestLocation { get; set; }
    }
}
