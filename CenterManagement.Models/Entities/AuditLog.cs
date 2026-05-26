using System;

namespace CenterManagement.Models.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;     // Create, Update, Delete, Login, Logout, ...
        public string EntityName { get; set; } = string.Empty; // Student, Teacher, Class, ...
        public int? EntityId { get; set; }
        public string? OldValues { get; set; }                  // JSON snapshot trước thay đổi
        public string? NewValues { get; set; }                  // JSON snapshot sau thay đổi
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
