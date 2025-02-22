using SQLite;

namespace TravelMate.Models
{
    /// Represents a user in the TravelMate application.
    /// This model is used to store flight details in the local SQLite database.
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }

    }
}