
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
                    _database.CreateTable<Checklist>();
                    _database.CreateTable<Place>();

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

        public static async Task<string> CheckCredentials(string email, string password)
        {
            var db = await GetDatabase();

            var user = db.Table<User>()
                               .FirstOrDefault(u => u.Email == email && u.Password == password);

            return user != null ? string.Empty : "Invalid email or password.";
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

        public static async Task<int> GetUserIdByEmail(string email)
        {
            var db = await GetDatabase();

            var user = db.Table<User>().FirstOrDefault(u => u.Email == email);

            return user?.Id ?? -1; // Return UserId if found, otherwise -1
        }

        public static async Task<bool> IsEmailExist(string email)
        {
            var db = await GetDatabase();
            var user = db.Table<User>().FirstOrDefault(u => u.Email == email);
            return user != null;
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
            Console.WriteLine($"userid in add hotel is  {userId}");
            hotel.UserId = userId; // Associate the hotel with the user
            var db = await GetDatabase();
            db.Insert(hotel);
        }

        public static async Task<List<Flight>> GetFlightsByUserId(int userId)
        {
            var db = await GetDatabase();
            // No need for await here, ToList() is synchronous in this context.
            var flights = db.Table<Flight>().Where(f => f.UserId == userId).ToList();

            if (flights.Any())
            {
                Console.WriteLine($"Found {flights.Count} flights for UserId {userId}.");
            }
            else
            {
                Console.WriteLine($"No flights found for UserId {userId}.");
            }
            return flights;
        }

        public static async Task<List<Hotel>> GetHotelsByUserId(int userId)
        {
            var db = await GetDatabase();
            var hotels = db.Table<Hotel>().Where(h => h.UserId == userId).ToList();
            return hotels;
        }

        public static async Task<List<Checklist>> GetChecklist(int userId, string destination)
        {
            var db = await GetDatabase();
            return db.Table<Checklist>()
                     .Where(i => i.UserId == userId && i.Destination == destination)
                     .ToList();
        }

        public static async Task AddChecklistItem(Checklist item)
        {
            var db = await GetDatabase();
            db.Insert(item);
        }

        public static async Task UpdateChecklistItem(Checklist item)
        {
            var db = await GetDatabase();
            db.Update(item);
        }

        public static async Task DeleteChecklistItem(Checklist item)
        {
            var db = await GetDatabase();
            db.Delete(item);
        }

        public static async Task<List<Place>> GetPlaces(int userId, string category)
        {
            var db = await GetDatabase();
            return db.Table<Place>()
                .Where(p => p.UserId == userId && p.Category == category)
                .ToList();
        }

        public static async Task AddPlaces(List<Place> places)
        {
            var db = await GetDatabase();
            db.InsertAll(places);
        }

        public static async Task<bool> UpdateUserName(int userId, string newName)
        {
            var db = await GetDatabase();

            var user = db.Table<User>().FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                user.Name = newName; // Update the user's name
                db.Update(user); // Save the updated user back to the database
                return true; // Return true if the update was successful
            }

            return false; // Return false if user was not found
        }

        public static async Task<bool> UpdateUserPassword(int userId, string oldPassword, string newPassword)
        {
            var db = await GetDatabase();

            var user = db.Table<User>().FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                // Check if the old password is correct
                if (user.Password == oldPassword)
                {
                    user.Password = newPassword; // Update the user's password
                    db.Update(user); // Save the updated user back to the database
                    return true; // Return true if the password was updated
                }
                else
                {
                    return false; // Return false if the old password does not match
                }
            }

            return false; // Return false if user was not found
        }


    }
}