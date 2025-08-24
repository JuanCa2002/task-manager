namespace TaskManager.Models.Entities
{
    public class StepEntity
    {
        public Guid Id { get; set; }
        public int TaskId { get; set; }
        public TaskEntity Task { get; set; } = null!;
        public string? Description { get; set; }
        public bool Done { get; set; }
        public int Order { get; set; }
    }
}
