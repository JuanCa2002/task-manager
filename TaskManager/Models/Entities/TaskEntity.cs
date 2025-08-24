using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.Entities
{
    public class TaskEntity
    {
        public int Id { get; set; }
        [StringLength(250)]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public DateTime CreationDate { get; set; }
        public List<StepEntity> Steps { get; set; } = [];
        public List<AttachedFileEntitty> AttachedFiles { get; set; } = [];
        public string UserOwnerId { get; set; } = string.Empty;
        public IdentityUser UserOwner { get; set; } = null!;
    }
}
