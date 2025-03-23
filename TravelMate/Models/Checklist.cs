using SQLite;

namespace TravelMate.Models
{
    public class Checklist
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Destination { get; set; }
        public string Item { get; set; }
        public string Category { get; set; }
        public bool IsChecked { get; set; }
    }
}
