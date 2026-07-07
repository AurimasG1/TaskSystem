namespace TaskSystem.Domain.Entities
{
    public class UserProfile
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Uzduotis> Uzduotys { get; set; } = new List<Uzduotis>();
    }
}
