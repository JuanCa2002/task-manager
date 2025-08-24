using Microsoft.EntityFrameworkCore;

namespace TaskManager.Models.Entities
{
    public class AttachedFileEntitty
    {
        public Guid Id { get; set; }
        public int TaskId { get; set; }
        public TaskEntity Task { get; set; } = null!;

        [Unicode(false)]
        public string Url { get; set; } = string.Empty;
        public string? Title { get; set; }
        public int Order { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
