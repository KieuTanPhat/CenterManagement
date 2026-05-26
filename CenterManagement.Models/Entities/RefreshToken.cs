using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenterManagement.Models.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string JwtId { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public bool IsUsed { get; set; } = false;
        public User User { get; set; } = null!;
    }
}
