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
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void SetTitle(string title)
        {
            ArgumentNullException.ThrowIfNull(title);
            _title = TaskTitle.Create(title);
            UpdatedAt = DateTime.UtcNow;
        }

        public string TitleValue
        {
            get => _title.Value;
            private set => _title = TaskTitle.Create(value);
        }

        public void Reset()
        {
            SetTitle("(reset) " + TitleValue);
            Description = null;
            StatusId = 1;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
