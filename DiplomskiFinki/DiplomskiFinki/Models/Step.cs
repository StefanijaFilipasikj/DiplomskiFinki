using System.ComponentModel.DataAnnotations;

namespace DiplomskiFinki.Models
{
    public class Step
    {
        [Key]
        public double SubStep { get; set; }
        public string SubStepName { get; set; }
        public virtual ICollection<DiplomaStatus> DiplomaStatuses { get; set; }
    }
}
