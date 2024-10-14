namespace ResondLawnSevices.Models
{
    public class Operator
    {
        public int Id { get; set; }
        public AppUser User { get; set; }
        public int UserId { get; set; }
        public Machine Machine { get; set; }
        public int MachineId { get; set; }
        public string Expertise { get; set; }
    }
}
