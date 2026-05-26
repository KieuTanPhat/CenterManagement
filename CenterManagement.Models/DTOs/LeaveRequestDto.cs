using CenterManagement.Models.Enums;

namespace CenterManagement.Models.DTOs
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int? ScheduleId { get; set; }
        public DateOnly RequestDate { get; set; }
        public DateOnly LeaveDate { get; set; }
        public string? Reason { get; set; }
        public LeaveRequestStatus Status { get; set; }
        public bool IsAbrupt { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string? ApprovedByName { get; set; }
    }

    public class CreateLeaveRequestDto
    {
        public int ClassId { get; set; }
        public int? ScheduleId { get; set; }
        public DateOnly LeaveDate { get; set; }
        public string? Reason { get; set; }
    }

    public class ApproveLeaveDto
    {
        public bool Approve { get; set; }
        public string? Note { get; set; }
    }
}
