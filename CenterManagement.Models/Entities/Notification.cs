using System;

namespace CenterManagement.Models.Entities
{
    public class Notification : BaseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? NotificationType { get; set; }      // System, ClassAlert, LeaveRequest, Payment, ...
        public bool IsRead { get; set; } = false;

        public int? TargetUserId { get; set; }             // null = broadcast tất cả
        public int CreatedByUserId { get; set; }

        public User? TargetUser { get; set; }
        public User CreatedBy { get; set; } = null!;
    }
}
