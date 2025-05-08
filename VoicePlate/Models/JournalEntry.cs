namespace VoicePlate.Models
{
    public class JournalEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string AudioPath { get; set; }
        public string PhotoPath { get; set; }
        public DateTime Timestamp { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationName { get; set; }
    }
}
