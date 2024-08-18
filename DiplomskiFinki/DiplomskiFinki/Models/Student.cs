using System.ComponentModel.DataAnnotations;

namespace DiplomskiFinki.Models
{
    public class Student
    {
        [Key]
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public int Credits { get; set; }
        public Diploma Diploma { get; set; }
    }
}
