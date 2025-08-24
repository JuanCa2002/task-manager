using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.Dtos
{
    public class TaskUpdateDTO
    {
        [Required]
        [StringLength(250)]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
