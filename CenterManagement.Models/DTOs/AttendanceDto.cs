using CenterManagement.Models.Enums;

namespace CenterManagement.Models.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class BulkAttendanceDto
    {
        public int ScheduleId { get; set; }
        public List<AttendanceItemDto> Attendances { get; set; } = new();
    }

    public class AttendanceItemDto
    {
        public int StudentId { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendanceRate { get; set; }
        public bool EligibleForFreeRetake { get; set; }
    }
}
