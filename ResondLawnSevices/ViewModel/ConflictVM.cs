namespace ResondLawnSevices.ViewModel
{
    public class ConflictVM
    {
        public int Id { get; set; } // Conflict ID
        public string UserId { get; set; } 
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; } 
        public int MachineId { get; set; } 
        public string MachineName { get; set; } 
        public DateTime RequestedDate { get; set; } 
        public string Status { get; set; } 
        public string AlternativeMachineName { get; set; }
    }
}
