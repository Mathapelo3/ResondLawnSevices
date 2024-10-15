using Azure;

namespace ResondLawnSevices.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public string ImageUrl { get; set; }
        public string Status { get; set; }

        public ICollection<Operator> Operators { get; set; }

       
    }
}
