namespace ResondLawnSevices.Models
{
    public class Conflicts
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int MachineId { get; set; }
        public DateTime RequestedDate { get; set; }
        public string Status { get; set; }

        //// New properties
        //public bool IsAcknowledged { get; set; }
        //public bool IsCompleted { get; set; }

        // Navigation property
        public virtual AppUser User { get; set; }
        public virtual Machine Machine { get; set; }
    }
}
