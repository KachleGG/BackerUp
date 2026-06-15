using System.ComponentModel.DataAnnotations.Schema;

namespace BackerUp.Admin.Server.Models.Entities
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("token_hash")]
        public string TokenHash { get; set; } = string.Empty;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        public User User { get; set; } = null!;
    }
}