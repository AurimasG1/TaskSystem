using TaskSystem.Domain.ValueObjects;

namespace TaskSystem.Domain.Entities
{
    public class Uzduotis
    {
        public int Id { get; set; }
        private TaskTitle _title = null!;
        public TaskTitle Title => _title;

        public string? Description { get; set; }
        public int StatusId { get; set; }
        public UzduotisStatus Status { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void SetTitle(string title)
        {
            _title = TaskTitle.Create(title);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
