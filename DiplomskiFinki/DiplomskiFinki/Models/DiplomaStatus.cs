using System.ComponentModel.DataAnnotations;

namespace DiplomskiFinki.Models
{
    public class DiplomaStatus
    {
        [Key]
        public Guid Id { get; set; }
        public Step? Step { get; set; }
        public Diploma? Diploma { get; set; }
        public bool Status { get; set; }
    }
}
