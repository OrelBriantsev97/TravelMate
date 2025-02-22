using SQLite;

namespace TravelMate.Models
{
    /// Represents a Hotel booking in the TravelMate application.
    /// This model is used to store flight details in the local SQLite database.
    public class Hotel
    {
        [PrimaryKey, AutoIncrement]
        public int HotelId { get; set; }
        public int UserId { get; set; }
        public string HotelName { get; set; }
        public string Address { get; set; }
        public string Phone {  get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string LogoUrl { get; set; }
        public int Class { get; set; }
        public string NearbyPlaces { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Amenities { get; set; }
    }
}
