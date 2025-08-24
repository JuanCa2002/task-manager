namespace TaskManager.Models.Dtos
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int DoneSteps { get; set; }
        public int TotalSteps { get; set; }
    }
}
