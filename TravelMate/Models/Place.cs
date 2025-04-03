using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMate.Models
{
    public class Place
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Destination { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } 
        public string Address { get; set; }
        public double Rating { get; set; }
        public string OpeningHours { get; set; }
        public string Thumbnail { get; set; }
    }

}
