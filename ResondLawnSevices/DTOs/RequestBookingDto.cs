using System.ComponentModel.DataAnnotations;

namespace ResondLawnSevices.DTOs
{
    public class RequestBookingDto
    {
        [Key]
        public int UserId { get; set; }
        public int MachineId { get; set; }
        public DateTime Date { get; set; }
    }
}
