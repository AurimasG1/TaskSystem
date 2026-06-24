namespace TaskSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "user";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Uzduotis> Uzduotys { get; set; } = new List<Uzduotis>();
    }
}
