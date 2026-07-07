namespace TaskSystem.Domain.Entities
{
    public class AdminPromotionToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
    }
}
