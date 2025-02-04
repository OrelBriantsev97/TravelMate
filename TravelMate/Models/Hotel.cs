using SQLite;

namespace TravelMate.Models
{
    public class Hotel
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string City { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string LogoUrl { get; set; }
        public double Rate { get; set; }
        public string NearbyPlaces { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
