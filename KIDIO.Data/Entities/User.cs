using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Common;
using KIDIO.Common.Enums;

namespace KIDIO.Data.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }         // null nếu OAuth
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; } = UserRole.Parent;

        // OAuth
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }

        // Refresh token
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation
        public ICollection<Child> Children { get; set; } = new List<Child>();
    }
}
