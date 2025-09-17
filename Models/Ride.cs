namespace Flutter_Backed.Models
{
    public class Ride
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string Color { get; set; } // Hex color code (e.g., "#4CAF50")
    }
    public class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public bool IsRead { get; set; }
    }
}
