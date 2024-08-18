using System.ComponentModel.DataAnnotations;

namespace DiplomskiFinki.Models
{
    public class Diploma
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Domain { get; set; }
        public string Description { get; set; }
        public Guid? StudentId { get; set; }
        public Student? Student { get; set; }
        public Guid? MentorId { get; set; }
        public Staff? Mentor { get; set; }
        public Guid? Member1Id { get; set; }
        public Staff? Member1 { get; set; }
        public Guid? Member2Id { get; set; }
        public Staff? Member2 { get; set; }
        public string? FilePath { get; set; }
        public Guid DiplomaStatusId { get; set; }
        public DiplomaStatus? DiplomaStatus { get; set; }
        public DateTime? ApplicationDate { get; set; }
        public DateTime? PresentationDate { get; set; }
        public string? Classroom { get; set; }
    }
}
