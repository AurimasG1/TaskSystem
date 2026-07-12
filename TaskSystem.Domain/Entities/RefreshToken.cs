namespace TaskSystem.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedAt { get; private set; }

    public string? RevocationReason { get; private set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Issuer { get; set; } = null!;

    public User User { get; set; } = null!;

    public void Revoke(DateTime revokedAtUtc, string reason)
    {
        if (IsRevoked)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Revocation reason is required.", nameof(reason));
        }

        IsRevoked = true;
        RevokedAt = revokedAtUtc;
        RevocationReason = reason.Trim();
    }
}
