using System.Text.Json.Serialization;

namespace CheckInFrontend.Models
{
    public class StudentData
    {
        public int StudentId { get; set; }
        public string CardUid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TeamName { get; set; }
        public bool IsCheckedIn { get; set; }
        public int EventId { get; set; }
        public CheckInStatus Status { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CheckInStatus
    {
        CheckedIn,
        AlreadyCheckedIn,
        StudentNotFound
    }
}
