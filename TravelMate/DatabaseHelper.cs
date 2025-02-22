
using SQLite;
using TravelMate.Models;
using System.Diagnostics;
namespace TravelMate
{
    public class DatabaseHelper
    {
        private static SQLiteConnection _database;

        public static async Task<SQLiteConnection> GetDatabase()
        {
            if (_database == null)
            {
                // Get the path to the database file
                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "travelmatedb.db");
                // Create a new SQLite connection
                _database = new SQLiteConnection(databasePath);

                try
                {
                    _database.CreateTable<User>();
                    _database.CreateTable<Flight>();
                    _database.CreateTable<Hotel>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating tables: {ex.Message}");
                }

            }
            return _database;
        }
        public static async Task AddNewUser(User user)
        {
            var db = await GetDatabase();
            try
            {
                db.Insert(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating tables: {ex.Message}");
            }

        }

        public static async Task<string> GetUserNameById(int userId)
        {
            string userName = string.Empty;

            // Using the async database connection
            var db = await GetDatabase();

            // Query the database to get the user by their ID
            var user = db.Table<User>().FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                userName = user.Name; // Return the user's name
            }

            return userName;
        }
        public static async Task<bool> IsEmailExist(string email)
        { 
            var db = await GetDatabase();  // Use the async method to get the database connection
            var user = db.Table<User>().FirstOrDefault(u => u.Email == email);  // Use async method for querying the table
            return user != null;  // Return whether a user with that email exists
        }

        public static async Task AddFlight(Flight flight, int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid UserId");
            }

            flight.UserId = userId;
            var db = await GetDatabase();
            db.Insert(flight);
        }

        public static async Task AddHotel(Hotel hotel, int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid UserId");
            }

            hotel.UserId = userId; // Associate the hotel with the user
            var db = await GetDatabase();
            db.Insert(hotel);
        }


    }
}
