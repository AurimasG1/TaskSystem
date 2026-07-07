using TaskSystem.Domain.ValueObjects;

namespace TaskSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        private Email _email = null!;
        public Email Email => _email;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = null!;
        public UserProfile Profile { get; set; } = null!;

        public void SetEmail(string email)
        {
            _email = Email.Create(email);
        }

        public string EmailValue
        {
            get => _email.Value;
            private set => _email = Email.Create(value);
        }
    }
}
