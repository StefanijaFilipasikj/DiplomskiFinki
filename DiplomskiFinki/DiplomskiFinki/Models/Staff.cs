using System.ComponentModel.DataAnnotations;

namespace DiplomskiFinki.Models
{
    public class Staff
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
    }
}   
