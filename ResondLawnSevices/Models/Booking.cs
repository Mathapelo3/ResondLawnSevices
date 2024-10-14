namespace ResondLawnSevices.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public AppUser User { get; set; }
        public string UserId { get; set; }
        public Machine Machine { get; set; }
        public int MachineId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public bool IsAcknowledged { get; set; }
        public bool IsCompleted { get; set; }
    }
}
