using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.Dtos
{
    public class StepCreateDTO
    {
        [Required]
        public string? Description { get; set; }
        public bool Done { get; set; }  
    }
}
