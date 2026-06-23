namespace TaskSystem.Domain.Entities
{
    public class Uzduotis
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int StatusId { get; set; }
        public UzduotisStatus Status { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void Reset()
        {
            Title = "(reset) " + Title;
            Description = null;
            StatusId = 1;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
