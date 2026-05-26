using CenterManagement.Models.Enums;

namespace CenterManagement.Models.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int ClassId { get; set; }
        public int? ScheduleId { get; set; }
        public DateOnly RequestDate { get; set; }
        public DateOnly LeaveDate { get; set; }
        public string? Reason { get; set; }
        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
        public bool IsAbrupt { get; set; } = false;
        public decimal PenaltyAmount { get; set; } = 0;
        public int? ApprovedByUserId { get; set; }

        public Teacher Teacher { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public Schedule? Schedule { get; set; }
        public User? ApprovedBy { get; set; }
    }
}
