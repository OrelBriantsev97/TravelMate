using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMate.Models
{
    public class HotelDetails
    {
        public string HotelName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public string LogoUrl { get; set; }
        public double Rate { get; set; }
        public string NearbyPlaces { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> Amenities { get; set; }
        public int HotelClass { get; set; }

        public ObservableCollection<string> HotelClassStars
        {
            get
            {
                return new ObservableCollection<string>(Enumerable.Repeat("star_icon.png", HotelClass));
            }
        }
    }
}
