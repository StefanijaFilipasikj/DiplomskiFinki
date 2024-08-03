using System.ComponentModel.DataAnnotations;

namespace DiplomskiFinki.Models
{
    public class Diploma
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Student { get; set; }
        public string Mentor { get; set; }
        public string Member1 { get; set; }
        public string Member2 { get; set; }
        public DateTime? PresentationDate { get; set; }
    }
}
