using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ResondLawnSevices.Models
{
    public class AppUser: IdentityUser
    {
        [StringLength(100)]
        [MaxLength(100)]
        [Required]

        

        public string? Name { get; set; }

        public string? Email { get; set; }
        public string? Address   { get; set; }

        public virtual ICollection<Conflicts> Conflicts { get; set; } = new List<Conflicts>(); 
    }
}
