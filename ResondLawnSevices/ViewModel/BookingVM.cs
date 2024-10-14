using ResondLawnSevices.Models;

namespace ResondLawnSevices.ViewModel
{
    public class BookingVM
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CustomerName { get; set; } // Customer name from AppUser
        public string CustomerEmail { get; set; } // Customer email from AppUser
        public string CustomerAddress { get; set; } // Customer address from AppUser
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public bool IsAcknowledged { get; set; }
        public bool IsCompleted { get; set; }
    }
}
