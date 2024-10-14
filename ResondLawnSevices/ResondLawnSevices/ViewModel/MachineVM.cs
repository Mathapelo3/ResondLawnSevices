namespace ResondLawnSevices.ViewModel
{
    public class MachineVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile Image { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
    }

}
