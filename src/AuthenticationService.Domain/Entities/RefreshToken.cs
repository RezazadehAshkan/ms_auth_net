using System;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public User User { get; private set; } = null!;

        [Required]
        [MaxLength(64)]
        public string TokenHash { get; private set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; private set; }

        [Required]
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        
        
        public DateTime? RevokedAt { get; private set; }

        public Guid? ReplacedByTokenId { get; private set; }

        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

        public RefreshToken()
        {
            Id = Guid.NewGuid();
        }        

        public RefreshToken(Guid userId, string hashedToken, DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UserId = userId;
            TokenHash = hashedToken;
            ExpiresAt = expiresAt;
        }

        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
        }

        public void ReplaceWith(Guid newTokenId)
        {
            Revoke();
            ReplacedByTokenId = newTokenId;
        }
    }
}