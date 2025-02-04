using SQLite;

namespace TravelMate.Models
{
    public class Flight
    {
        [PrimaryKey, AutoIncrement]
        public int FlightId { get; set; }
        public int UserId { get; set; }

        public string Airline {  get; set; }
        public string FlightNumber { get; set; }
        public string DepartureTime { get; set; }
        public string DepartureDate { get; set; }
        public string ArrivalTime { get; set; }
        public string ArrivalDate { get; set; }
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public string Duration { get; set; }
    }
}
