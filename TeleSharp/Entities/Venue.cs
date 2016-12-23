using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSharp.Entities
{
    public class Venue
    {
        public Location Location { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string FoursquareId { get; set; }
    }
}
